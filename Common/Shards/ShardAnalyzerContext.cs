using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafe;

public record struct ShardAnalyzerContext(
    Type ShardType,
    Uri ShardUri,
    string? MimeType,
    string? UploadFilename
);
