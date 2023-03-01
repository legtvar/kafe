using Kafe.Media;
using Kafe.Ruv;
using System.Collections.Immutable;

var mediaClient = new FFmpegCoreService();
var info = await mediaClient.GetInfo("");
Console.WriteLine(info);
