using Kafe.Data.Options;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public class DefaultStorageService : IStorageService
{
    private readonly IOptions<StorageOptions> options;

    public DefaultStorageService(IOptions<StorageOptions> options)
    {
        this.options = options;
    }

    public async Task<bool> TryStoreShard(
        ShardKind kind,
        Hrib id,
        Stream stream,
        string? variant,
        string fileExtension,
        CancellationToken token = default)
    {
        try
        {
            variant ??= Const.OriginalShardVariant;
            var storageDir = RequireShardDirectory(kind, create: true);
            var shardDir = storageDir.CreateSubdirectory(id);
            var originalPath = Path.Combine(shardDir.FullName, $"{variant}{fileExtension}");
            using var originalStream = new FileStream(originalPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(originalStream, token);
        }
        catch(IOException)
        {
            return false;
        }

        return true;
    }

    public bool TryOpenShardStream(
        ShardKind kind,
        Hrib id,
        string? variant,
        [NotNullWhen(true)] out Stream? stream,
        [NotNullWhen(true)] out string? fileExtension)
    {
        variant ??= Const.OriginalShardVariant;

        var storageDir = RequireShardDirectory(kind, create: false);

        var shardDir = new DirectoryInfo(Path.Combine(storageDir.FullName, id));
        if (!shardDir.Exists)
        {
            throw new ArgumentException($"Shard directory '{id}' could not be found.");
        }

        var variantFiles = shardDir.GetFiles($"{variant}.*");
        if (variantFiles.Length == 0)
        {
            throw new ArgumentException($"The '{variant}' variant of shard '{id}' could not be found.");
        }
        else if (variantFiles.Length > 1)
        {
            throw new ArgumentException($"The '{variant}' variant of shard '{id}' has multiple source " +
                "files. This is probably a bug.");
        }

        try
        {
            var variantFile = variantFiles.Single();
            stream = variantFile.OpenRead();
            fileExtension = variantFile.Extension;
            return true;
        }
        catch (Exception e) when (e is UnauthorizedAccessException || e is IOException)
        {
            stream = null;
            fileExtension = null;
            return false;
        }
    }

    private DirectoryInfo RequireShardDirectory(ShardKind kind, bool create = true)
    {
        if (!options.Value.ShardDirectories.TryGetValue(kind, out var dirPath))
        {
            throw new ArgumentNullException($"The storage directory for the '{kind}' shard kind is not set.");
        }

        var info = new DirectoryInfo(dirPath);
        if (!info.Exists)
        {
            if (create)
            {
                info.Create();
            }
            else
            {
                throw new ArgumentException($"The '{dirPath}' '{kind}' shard storage directory does not exist.");
            }
        }

        return info;
    }
}
