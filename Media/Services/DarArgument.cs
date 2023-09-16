using System.Drawing;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace Kafe.Media.Services;

internal class DarArgument : IVideoFilterArgument
{
    public readonly Size? Size;
    public DarArgument(Size? size)
    {
        Size = size;
    }

    public DarArgument(int width, int height) : this(new Size(width, height)) { }

    public DarArgument(VideoSize videosize)
    {
        Size = videosize == VideoSize.Original ? null : (Size?)new Size(-1, (int)videosize);
    }

    public string Key { get; } = "setdar";
    public string Value => Size == null ? string.Empty : $"{Size.Value.Width}:{Size.Value.Height}";
}
