using Kafe.Data.Options;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Core.Diagnostics;
using Kafe.Data.Diagnostics;

namespace Kafe.Data.Services;

public class StorageService(
    IOptions<StorageOptions> options,
    KafeTypeRegistry typeRegistry
)
{
    public async Task<Err<Uri>> StoreShard(
        Type shardType,
        Hrib id,
        Stream stream,
        string? variant,
        string fileExtension,
        CancellationToken token = default
    )
    {
        string? filePath = null;
        try
        {
            variant ??= Const.OriginalShardVariant;
            var storageDir = GetShardTypeDirectory(shardType, create: true);
            storageDir.EnumerateDirectories()
            var shardDir = storageDir.CreateSubdirectory(id.ToString());
            filePath = Path.Combine(shardDir.FullName, $"{variant}{fileExtension}");
            using var originalStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(originalStream, token);
        }
        catch (IOException)
        {
            if (filePath is not null && File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return false;
        }

        return true;
    }

    public bool TryOpenShardStream(
        Type shardType,
        Hrib id,
        string? variant,
        [NotNullWhen(true)] out Stream? stream,
        [NotNullWhen(true)] out string? fileExtension
    )
    {
        if (!TryGetShardFilePath(shardType, id, variant, out var filePath))
        {
            stream = null;
            fileExtension = null;
            return false;
        }

        try
        {
            var info = new FileInfo(filePath);
            stream = info.OpenRead();
            fileExtension = info.Extension;
            return true;
        }
        catch (Exception e) when (e is UnauthorizedAccessException || e is IOException)
        {
            stream = null;
            fileExtension = null;
            return false;
        }
    }

    public Err<Uri> GetShardUri(
        Hrib id,
        Type? shardType,
        string? variant
    )
    {
        variant ??= Const.OriginalShardVariant;



        var storageDir = GetShardTypeDirectory(
            shardType: shardType,
            variant: variant ?? Const.OriginalShardVariant,
            create: false
        );

        var shardDir = new DirectoryInfo(Path.Combine(storageDir.FullName, id.ToString()));
        if (!shardDir.Exists)
        {
            return shouldThrow
                ? throw new ArgumentException($"Shard directory '{id}' could not be found.")
                : false;
        }

        var variantFiles = shardDir.GetFiles($"{variant}.*");
        if (variantFiles.Length == 0)
        {
            return shouldThrow
                ? throw new ArgumentException($"The '{variant}' variant of shard '{id}' could not be found.")
                : false;
        }
        else if (variantFiles.Length > 1)
        {
            return shouldThrow
                ? throw new ArgumentException(
                    $"The '{variant}' variant of shard '{id}' has multiple source " +
                    "files. This is probably a bug."
                )
                : false;
        }

        var variantFile = variantFiles.Single();
        filePath = variantFile.FullName;
        return true;
    }

    public async Task<Err<Uri>> StoreTemporaryShard(
        Hrib id,
        Stream stream,
        string fileExtension,
        CancellationToken ct = default
    )
    {
        if (!id.IsValidNonEmpty)
        {
            throw new ArgumentException("The shard's ID has to be valid and non-empty.", nameof(id));
        }

        var tmpFilename = $"{id}{fileExtension}";
        var tmpPath = Path.Combine(options.Value.TempDirectory, tmpFilename);
        try
        {
            if (!Directory.Exists(options.Value.TempDirectory))
            {
                Directory.CreateDirectory(options.Value.TempDirectory);
            }

            var existingPath = GetTemporaryShardUri(id);
            if (existingPath is { HasError: true, Diagnostic.Payload: TemporaryShardNotFoundDiagnostic })
            {
                throw new InvalidOperationException(
                    $"Temporary shard '{id}' has already been stored. This is likely a bug."
                );
            }

            if (existingPath.HasError)
            {
                return existingPath;
            }

            await using var tmpFile = File.OpenWrite(tmpPath);
            await stream.CopyToAsync(tmpFile, ct);
        }
        catch (IOException)
        {
            if (File.Exists(tmpPath))
            {
                File.Delete(tmpPath);
            }

            throw;
        }

        return new Uri($"{Const.TemporaryAccountPurpose}://{tmpFilename}");
    }

    public Err<Uri> GetTemporaryShardUri(Hrib id)
    {
        var tmpDir = new DirectoryInfo(options.Value.TempDirectory);
        if (!tmpDir.Exists)
        {
            return Err.Fail(new TemporaryShardNotFoundDiagnostic(id));
        }

        var variantFiles = tmpDir.GetFiles($"{id}.*");
        if (variantFiles.Length == 0)
        {
            return Err.Fail(new TemporaryShardNotFoundDiagnostic(id));
        }

        if (variantFiles.Length > 1)
        {
            throw new ArgumentException(
                $"The temporary shard '{id}' has multiple source files. This is likely a bug."
            );
        }

        var variantFile = variantFiles.Single();
        return new Uri($"{Const.TemporaryAccountPurpose}://{Path.GetFileName(variantFile.FullName)}");
    }

    public Err<bool> DeleteTemporaryShard(Hrib id, CancellationToken ct = default)
    {
        if (id.IsInvalid)
        {
            return Err.Fail(new BadHribDiagnostic(id.RawValue));
        }

        if (id.IsEmpty)
        {
            return Err.Fail(new EmptyHribDiagnostic());
        }

        var tmpUri = GetTemporaryShardUri(id);
        if (tmpUri.HasError)
        {
            return tmpUri.Diagnostic;
        }

        var tmpPath = Path.Combine(options.Value.TempDirectory, tmpUri.Value.AbsolutePath);
        if (!File.Exists(tmpPath))
        {
            return Err.Fail(new TemporaryShardNotFoundDiagnostic(id));
        }

        File.Delete(tmpPath);
        return true;
    }

    public Err<Uri> MoveTemporaryToArchive(
        Hrib tmpShardId,
        Type shardType,
        string? fileExtension,
        string? variant = Const.OriginalShardVariant
    )
    {
        if (tmpShardId.IsInvalid)
        {
            return Err.Fail(new BadHribDiagnostic(tmpShardId.RawValue));
        }

        if (tmpShardId.IsEmpty)
        {
            return Err.Fail(new EmptyHribDiagnostic());
        }

        var tmpUri = GetTemporaryShardUri(tmpShardId);
        if (tmpUri.HasError)
        {
            return tmpUri.Diagnostic;
        }

        var tmpPath = tmpUri.Value.AbsolutePath;
        fileExtension ??= Path.GetExtension(tmpUri.Value.AbsolutePath);
        variant ??= Const.OriginalShardVariant;
        var storageDir = GetShardTypeDirectory(shardType, create: true);
        var shardDir = storageDir.CreateSubdirectory(tmpShardId.ToString());
        var newPath = Path.Combine(shardDir.FullName, $"{variant}{fileExtension}");
        try
        {
            File.Move(
                sourceFileName: tmpPath,
                destFileName: newPath
            );
        }
        catch (IOException)
        {
            if (File.Exists(newPath))
            {
                File.Delete(newPath);
            }

            throw;
        }

        return new Uri($"{Const.ArchiveFileScheme}://{newPath}");
    }

    public DirectoryInfo GetShardTypeDirectory(
        Type shardType,
        string variant = Const.OriginalShardVariant,
        bool create = true
    )
    {
        var baseDir = variant == Const.OriginalShardVariant
            ? options.Value.ArchiveDirectory
            : options.Value.GeneratedDirectory;

        var shardKafeType = typeRegistry.RequireType(shardType);
        if (!options.Value.ShardDirectories.TryGetValue(shardKafeType, out var shardDir))
        {
            throw new ArgumentNullException($"The storage directory for the '{shardType}' shard type is not set.");
        }

        var fullDir = Path.Combine(baseDir, shardDir);

        var info = new DirectoryInfo(fullDir);
        if (!info.Exists)
        {
            if (create)
            {
                info.Create();
            }
            else
            {
                throw new ArgumentException(
                    $"The '{shardType}' shard storage directory does not exist at "
                    + $"'{fullDir}'."
                );
            }
        }

        return info;
    }
}
