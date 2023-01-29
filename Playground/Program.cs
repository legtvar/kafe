using Kafe.Media;

var samplePath = @"";

var ffcore = new FFmpegCoreService();
var ffcoreTest = await ffcore.GetInfo(samplePath);

var xabe = new XabeFFmpegService();
var xabeTest = await xabe.GetInfo(samplePath);

Console.WriteLine($"FFMpegCore: {ffcoreTest}");
Console.WriteLine($"Xabe.FFmpeg: {xabeTest}");
