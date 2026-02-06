using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Kafe.Data.Diagnostics;
using Kafe.Data.Services;

namespace Kafe.Data;

public class LocalFindShardFile(StorageService storageService) : IFindShardFile
{
    public Task<Err<string>> Find(Uri shardUri, CancellationToken ct = default)
    {
        var path = storageService.GetAbsolutePath(shardUri);
        if (!File.Exists(path))
        {
            return Task.FromResult(Err.Fail<string>(new ShardFileNotFoundDiagnostic(Hrib.Empty)));
        }

        return Task.FromResult(Err.Succeed(storageService.GetAbsolutePath(shardUri)));
    }
}
