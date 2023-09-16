﻿using Kafe.Data.Options;
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
            var storageDir = GetShardDirectory(kind, create: true);
            var shardDir = storageDir.CreateSubdirectory(id);
            var originalPath = Path.Combine(shardDir.FullName, $"{variant}{fileExtension}");
            using var originalStream = new FileStream(originalPath, FileMode.Create, FileAccess.Write);
            await stream.CopyToAsync(originalStream, token);
        }
        catch (IOException)
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
        if (!TryGetFilePath(kind, id, variant, out var filePath))
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

    public bool TryGetFilePath(ShardKind kind, Hrib id, string? variant, out string filePath)
    {
        variant ??= Const.OriginalShardVariant;

        var storageDir = GetShardDirectory(kind, create: false);

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

        var variantFile = variantFiles.Single();
        filePath = variantFile.FullName;
        return true;
    }

    public bool TryDeleteShard(
        ShardKind kind,
        Hrib id,
        string variant)
    {
        if (variant == Const.OriginalShardVariant)
        {
            throw new InvalidOperationException("An original shard cannot be deleted.");
        }

        if (TryGetFilePath(kind, id, variant, out var path))
        {
            File.Delete(path);
            return true;
        }

        return false;
    }

    public DirectoryInfo GetShardDirectory(
        ShardKind kind,
        string variant = Const.OriginalShardVariant,
        bool create = true)
    {
        var baseDir = variant == Const.OriginalShardVariant
            ? options.Value.ArchiveDirectory
            : options.Value.GeneratedDirectory;

        if (!options.Value.ShardDirectories.TryGetValue(kind, out var shardDir))
        {
            throw new ArgumentNullException($"The storage directory for the '{kind}' shard kind is not set.");
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
                throw new ArgumentException($"The '{kind}' shard storage directory does not exist at '{fullDir}'.");
            }
        }

        return info;
    }
}
