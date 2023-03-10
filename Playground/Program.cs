using Kafe.Media.Services;
using Kafe.Ruv;
using System.Collections.Immutable;

var mediaClient = new FFmpegCoreService();
var info = await mediaClient.GetInfo("C:/Users/eidam/Downloads/SampleVideo_1280x720_10mb.mkv");
Console.WriteLine(info);
