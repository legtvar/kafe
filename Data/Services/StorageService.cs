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
using System.Collections.Immutable;

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
        string? fullShardPath = null;
        try
        {
            variant ??= Const.OriginalShardVariant;
            var storageDirUri = GetShardTypeDirectory(shardType, ensureExists: true);
            var subdirPath = Path.Combine(GetAbsolutePath(storageDirUri), id.ToString());
            var shardDir = new DirectoryInfo(subdirPath);
            if (!shardDir.Exists)
            {
                shardDir.Create();
            }

            var filename = $"{variant}{fileExtension}";
            fullShardPath = Path.Combine(shardDir.FullName, filename);
            if (File.Exists(fullShardPath))
            {
                return Err.Fail(new ShardFileAlreadyExists(id));
            }

            using var originalStream = new FileStream(fullShardPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(originalStream, token);
            var relativePath = Path.Combine(storageDirUri.AbsolutePath, id.ToString(), filename);
            return new Uri($"{storageDirUri.Scheme}://{relativePath}");
        }
        catch (IOException)
        {
            if (fullShardPath is not null && File.Exists(fullShardPath))
            {
                File.Delete(fullShardPath);
            }

            throw;
        }
    }

    public bool TryOpenShardStream(
        Hrib id,
        Type? shardType,
        string? variant,
        [NotNullWhen(true)] out Stream? stream,
        [NotNullWhen(true)] out string? fileExtension
    )
    {
        var shardUri = GetShardUri(id, shardType, variant);
        if (shardUri.HasError)
        {
            stream = null;
            fileExtension = null;
            return false;
        }

        try
        {
            var filePath = GetAbsolutePath(shardUri.Value);
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
        Type? shardType = null,
        string? variant = Const.OriginalShardVariant
    )
    {
        variant ??= Const.OriginalShardVariant;

        DirectoryInfo? shardDir = null;
        Uri? shardDirUri = null;

        if (shardType is not null)
        {
            var storageDirUri = GetShardTypeDirectory(shardType, variant, ensureExists: false);
            var storageDir = new DirectoryInfo(GetAbsolutePath(storageDirUri));
            if (!storageDir.Exists)
            {
                return Err.Fail(new ShardFileNotFoundDiagnostic(id));
            }

            shardDir = new DirectoryInfo(Path.Combine(storageDir.FullName, id.ToString()));
            shardDirUri = new Uri(
                $"{storageDirUri.Scheme}://{Path.Combine(storageDirUri.AbsolutePath, id.ToString())}"
            );
        }
        else
        {
            var baseScheme = variant == Const.OriginalShardVariant
                ? Const.ArchiveFileScheme
                : Const.GeneratedFileScheme;
            var baseDir = variant == Const.OriginalShardVariant
                ? new DirectoryInfo(options.Value.ArchiveDirectory)
                : new DirectoryInfo(options.Value.GeneratedDirectory);
            var shardDirCandidates = baseDir.EnumerateDirectories($"*/{id}").ToImmutableArray();
            if (shardDirCandidates.Length == 0)
            {
                return Err.Fail(new ShardFileNotFoundDiagnostic(id));
            }

            if (shardDirCandidates.Length > 1)
            {
                throw new InvalidOperationException(
                    $"File for shard '{id}' has been located in multiple subdirectories. This is likely a bug."
                );
            }

            shardDir = shardDirCandidates[0];
            shardDirUri = new Uri($"{baseScheme}://{Path.GetRelativePath(baseDir.FullName, shardDir.FullName)}");
        }

        if (!shardDir.Exists)
        {
            return Err.Fail(new ShardFileNotFoundDiagnostic(id));
        }

        var variantFiles = shardDir.GetFiles($"{variant}.*");
        if (variantFiles.Length == 0)
        {
            return Err.Fail(new ShardFileNotFoundDiagnostic(id));
        }
        else if (variantFiles.Length > 1)
        {
            throw new ArgumentException(
                $"The '{variant}' variant of shard '{id}' has multiple source files. This is probably a bug."
            );
        }

        var variantFile = variantFiles[0];
        return new Uri($"{shardDirUri.Scheme}://{Path.Combine(shardDirUri.AbsolutePath, variantFile.Name)}");
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
            if (existingPath is { HasError: true, Diagnostic.Payload: ShardFileNotFoundDiagnostic })
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
            return Err.Fail(new ShardFileNotFoundDiagnostic(id));
        }

        var variantFiles = tmpDir.GetFiles($"{id}.*");
        if (variantFiles.Length == 0)
        {
            return Err.Fail(new ShardFileNotFoundDiagnostic(id));
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
            return Err.Fail(new ShardFileNotFoundDiagnostic(id));
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
        var storageDirUri = GetShardTypeDirectory(shardType, ensureExists: true);
        var storageDir = new DirectoryInfo(GetAbsolutePath(storageDirUri));
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

    public Uri GetShardTypeDirectory(
        Type shardType,
        string variant = Const.OriginalShardVariant,
        bool ensureExists = true
    )
    {
        var scheme = variant == Const.OriginalShardVariant
            ? Const.ArchiveFileScheme
            : Const.GeneratedFileScheme;

        var shardKafeType = typeRegistry.RequireType(shardType);
        if (!options.Value.ShardDirectories.TryGetValue(shardKafeType, out var shardDir))
        {
            // TODO: Merge the per-type directories. It's more pain than it's worth. (Issue #273)
            throw new ArgumentNullException($"The storage directory for the '{shardType}' shard type is not set.");
        }

        var uri = new Uri($"{scheme}://{shardDir}");

        if (ensureExists)
        {
            var dirPath = GetAbsolutePath(uri);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }

        return uri;
    }

    public string GetAbsolutePath(Uri shardUri)
    {
        return shardUri.Scheme switch
        {
            Const.ArchiveFileScheme => Path.Combine(options.Value.ArchiveDirectory, shardUri.AbsolutePath),
            Const.GeneratedFileScheme => Path.Combine(options.Value.GeneratedDirectory, shardUri.AbsolutePath),
            Const.TempFileScheme => Path.Combine(options.Value.TempDirectory, shardUri.AbsolutePath),
            _ => throw new NotSupportedException(
                $"There is no storage directory defined for URI scheme '{shardUri.Scheme}'."
            )
        };
    }
}
