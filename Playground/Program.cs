using Kafe.Media;
using Kafe.Ruv;
using System.Collections.Immutable;

var mediaClient = new FFmpegCoreService();
var info = await mediaClient.GetInfo("C:/Users/eidam/Downloads/SampleVideo_1280x720_10mb_corrupted.mp4");
Console.WriteLine(info);
