
using FFMpegCore;
using Kafe.Media;
using Xabe.FFmpeg;

FFmpeg.SetExecutablesPath(@"C:\Users\Adam\scoop\shims");

GlobalFFOptions.Configure(new FFOptions { BinaryFolder = @"C:\Users\Adam\scoop\shims" });

var samplePath = @"";

var ffcore = new FFMpegCoreService();
var ffcoreTest = await ffcore.GetInfo(samplePath);

var xabe = new XabeFFmpegService();
var xabeTest = await xabe.GetInfo(samplePath);

Console.WriteLine($"FFMpegCore: {ffcoreTest}");
Console.WriteLine($"Xabe.FFmpeg: {xabeTest}");
