using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Data.Services;

public interface IStorageService
{
    Task<bool> TryStoreShard(
        ShardKind kind,
        Hrib id,
        Stream stream,
        string? variant,
        string fileExtension,
        CancellationToken token = default);

    bool TryOpenShardStream(
        ShardKind kind,
        Hrib id,
        string? variant,
        [NotNullWhen(true)] out Stream? stream,
        [NotNullWhen(true)] out string? fileExtension);

    bool TryGetFilePath(
        ShardKind kind,
        Hrib id,
        string? variant,
        [NotNullWhen(true)] out string? filePath);

    bool TryDeleteShard(
        ShardKind kind,
        Hrib id,
        string variant);
}
