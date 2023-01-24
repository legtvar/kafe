
using Kafe.Media;
using Xabe.FFmpeg;

FFmpeg.SetExecutablesPath(@"C:\Users\Adam\scoop\shims");

var service = new XabeFFmpegService();
var info = await service.GetInfo(@"");

