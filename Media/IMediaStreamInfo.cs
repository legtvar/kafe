namespace Kafe.Media;

public interface IMediaStreamInfo
{
    string Codec { get; }
    long Bitrate { get; }
}
