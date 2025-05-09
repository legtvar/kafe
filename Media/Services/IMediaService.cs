﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Kafe.Media.Services;

public interface IMediaService
{
    public const string SDFileName = "sd";
    public const string HDFileName = "hd";
    public const string FullHDFileName = "fullhd";
    public const string OriginalFileName = "original";

    Task<MediaInfo> GetInfo(string filePath, CancellationToken token = default);

    Task<MediaInfo> GetInfo(Stream stream, CancellationToken token = default);
    
    Task<MediaInfo> CreateVariant(
        string filePath,
        VideoQualityPreset preset,
        string? outputDir = null,
        bool overwrite = false,
        CancellationToken token = default);
}
