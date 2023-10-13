using Kafe.Media.Services;
using Kafe.Ruv;
using System.Collections.Immutable;

var mediaClient = new FFmpegCoreService();
var info = await mediaClient.CreateVariant(
    "C:/dev/fi/kafe/Samples/SampleVideo_1024x768_5mb.mp4",
    Kafe.Media.VideoQualityPreset.SD);
Console.WriteLine(info);
