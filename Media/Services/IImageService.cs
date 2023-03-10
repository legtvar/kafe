using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Media.Services;

public interface IImageService
{
    Task<ImageInfo> GetInfo(string filePath, CancellationToken token = default);

    Task<ImageInfo> GetInfo(Stream stream, CancellationToken token = default);
}
