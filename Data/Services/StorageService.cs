using Kafe.Data.Options;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Kafe.Data.Services;

// TODO: This class should either throw more exceptions or return Err<T>
public class StorageService(
    IOptions<StorageOptions> options,
    KafeTypeRegistry typeRegistry
)
{
    public async Task<bool> TryStoreShard(
        Type shardType,
        Hrib id,
        Stream stream,
        string? variant,
        string fileExtension,
        CancellationToken token = default)
    {
        string? originalPath = null;
        try
        {
            variant ??= Const.OriginalShardVariant;
            var storageDir = GetShardTypeDirectory(shardType, create: true);
            var shardDir = storageDir.CreateSubdirectory(id.ToString());
            originalPath = Path.Combine(shardDir.FullName, $"{variant}{fileExtension}");
            using var originalStream = new FileStream(originalPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(originalStream, token);
        }
        catch (IOException)
        {
            if (originalPath is not null && File.Exists(originalPath))
            {
                File.Delete(originalPath);
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
        [NotNullWhen(true)] out string? fileExtension)
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

    public bool TryGetShardFilePath(
        Type shardType,
        Hrib id,
        string? variant,
        [NotNullWhen(true)] out string? filePath,
        bool shouldThrow = false)
    {
        variant ??= Const.OriginalShardVariant;
        filePath = null;

        var storageDir = GetShardTypeDirectory(
            shardType: shardType,
            variant: variant ?? Const.OriginalShardVariant,
            create: false);

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
            return shouldThrow ?
                throw new ArgumentException($"The '{variant}' variant of shard '{id}' has multiple source " +
                    "files. This is probably a bug.")
                : false;
        }

        var variantFile = variantFiles.Single();
        filePath = variantFile.FullName;
        return true;
    }

    public async Task<string> StoreTemporaryShard(
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

        string? tmpPath = null;
        try
        {
            if (!Directory.Exists(options.Value.TempDirectory))
            {
                Directory.CreateDirectory(options.Value.TempDirectory);
            }

            var existingPath = GetTemporaryShardFilePath(id);
            if (existingPath is not null)
            {
                throw new ArgumentException($"Temporary shard '{id}' already exists.");
            }

            tmpPath = Path.Combine(options.Value.TempDirectory, $"{id}{fileExtension}");
            using var tmpFile = File.OpenWrite(tmpPath);
            await stream.CopyToAsync(tmpFile, ct);
        }
        catch (IOException)
        {
            if (tmpPath is not null && File.Exists(tmpPath))
            {
                File.Delete(tmpPath);
            }

            throw;
        }

        return tmpPath;

    }

    public string? GetTemporaryShardFilePath(Hrib id, bool shouldThrow = false)
    {
        var tmpDir = new DirectoryInfo(options.Value.TempDirectory);
        if (!tmpDir.Exists)
        {
            return null;
        }

        var variantFiles = tmpDir.GetFiles($"{id}.*");
        if (variantFiles.Length == 0)
        {
            return shouldThrow
                ? throw new ArgumentException($"The temporary shard '{id}' could not be found.")
                : null;
        }
        else if (variantFiles.Length > 1)
        {
            return shouldThrow ?
                throw new ArgumentException($"The temporary shard '{id}' has multiple source " +
                    "files. This is probably a bug.")
                : null;
        }

        var variantFile = variantFiles.Single();
        return variantFile.FullName;
    }

    public Task DeleteTemporaryShard(Hrib id, CancellationToken ct = default)
    {
        if (!id.IsValidNonEmpty)
        {
            throw new ArgumentException("The shard's ID has to be valid and non-empty.", nameof(id));
        }

        var tmpPath = GetTemporaryShardFilePath(id);
        if (tmpPath is not null && File.Exists(tmpPath))
        {
            File.Delete(tmpPath);
        }
        return Task.CompletedTask;
    }

    public Task MoveTemporaryShard(
        Hrib id,
        Type shardType,
        string fileExtension,
        string variant = Const.OriginalShardVariant,
        CancellationToken ct = default
    )
    {
        if (!id.IsValidNonEmpty)
        {
            throw new ArgumentException("The shard's ID has to be valid and non-empty.", nameof(id));
        }

        var tmpPath = GetTemporaryShardFilePath(id, true);
        if (tmpPath is null)
        {
            throw new ArgumentException($"Temporary shard '{id}' could not be found in the file system.");
        }

        variant ??= Const.OriginalShardVariant;
        var storageDir = GetShardTypeDirectory(shardType, create: true);
        var shardDir = storageDir.CreateSubdirectory(id.ToString());
        var originalPath = Path.Combine(shardDir.FullName, $"{variant}{fileExtension}");
        File.Move(
            sourceFileName: tmpPath,
            destFileName: originalPath
        );

        return Task.CompletedTask;
    }

    public DirectoryInfo GetShardTypeDirectory(
        Type shardType,
        string variant = Const.OriginalShardVariant,
        bool create = true)
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
                throw new ArgumentException($"The '{shardType}' shard storage directory does not exist at "
                    + $"'{fullDir}'.");
            }
        }

        return info;
    }
}
