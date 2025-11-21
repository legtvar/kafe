using System.Drawing;
using System.Text;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;

namespace Kafe.Media.Services;

internal class ScaleArgument : IVideoFilterArgument
{
    public ScaleArgument(
        Size? size,
        string? forceOriginalAspectRatio = null,
        int forceDivisibleBy = 0,
        bool resetSar = false
    )
    {
        Size = size;
        ForceOriginalAspectRatio = forceOriginalAspectRatio;
        ForceDivisibleBy = forceDivisibleBy;
        ResetSar = resetSar;

        var sb = new StringBuilder();
        sb.Append(size?.Width ?? -2);
        sb.Append(':');
        sb.Append(size?.Height ?? -2);

        if (!string.IsNullOrEmpty(forceOriginalAspectRatio))
        {
            sb.Append(':');
            sb.Append("force_original_aspect_ratio=");
            sb.Append(forceOriginalAspectRatio);
        }

        if (forceDivisibleBy > 0)
        {
            sb.Append(':');
            sb.Append("force_divisible_by=");
            sb.Append(forceDivisibleBy);
        }

        if (resetSar)
        {
            sb.Append(":reset_sar=true");
        }

        Value = sb.ToString();
    }

    public ScaleArgument(
        int width,
        int height,
        string? forceOriginalAspectRatio = null,
        int forceDivisibleBy = 0,
        bool resetSar = false
    ) : this(
        new Size(width, height),
        forceOriginalAspectRatio,
        forceDivisibleBy,
        resetSar
    )
    {
    }

    public Size? Size { get; }

    private string? ForceOriginalAspectRatio { get; }
    public int ForceDivisibleBy { get; }
    public bool ResetSar { get; }

    public string Key => "scale";

    public string Value { get; }
}
