using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

public record struct ShardAnalyzerContext(
    Type ShardType,
    // NB: Shard analyzers get only the URI and not the shard ID, because the shard may not be persisted yet.
    //     For practical reasons, we don't want to store corrupted files that cannot be analyzed or fail some hard
    //     requirements.
    Uri ShardUri,
    string? MimeType,
    string? UploadFilename
);
