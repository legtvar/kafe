using System;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public enum MediaBitrateKind
{
    Total,
    Video,
    Audio,
    Subtitles
}

public record MediaBitrateRequirement(
    int? Min,
    int? Max,
    MediaBitrateKind Kind,
    int? StreamIndex
) : IRequirement
{
    public static string Moniker { get; } = "media-bitrate";
}

public class MediaBitrateRequirementHandler : ShardRequirementHandlerBase<MediaBitrateRequirement>
{
    public override ValueTask Handle(IShardRequirementContext<MediaBitrateRequirement> context)
    {
        if (context.Requirement.Min is null && context.Requirement.Max is null)
        {
            // TODO: warn about the uselessness of the user's doing.
            return ValueTask.CompletedTask;
        }

        var mediaInfo = context.RequireMediaInfo();
        if (mediaInfo is null)
        {
            return ValueTask.CompletedTask;
        }

        switch (context.Requirement.Kind)
        {
            case MediaBitrateKind.Total:
                if (context.Requirement.Min.HasValue && mediaInfo.Bitrate < context.Requirement.Min)
                {
                    context.Report(new MediaBitrateTooLowDiagnostic(
                        ShardName: context.Shard.Name,
                        ShardId: context.Shard.Id,
                        Variant: null,
                        Min: context.Requirement.Min.Value
                    ));
                    return ValueTask.CompletedTask;
                }
                if (context.Requirement.Max.HasValue && mediaInfo.Bitrate > context.Requirement.Max)
                {
                    context.Report(new MediaBitrateTooHighDiagnostic(
                        ShardName: context.Shard.Name,
                        ShardId: context.Shard.Id,
                        Variant: null,
                        Max: context.Requirement.Max.Value
                    ));
                    return ValueTask.CompletedTask;
                }
                break;

            case MediaBitrateKind.Video:
                if (context.Requirement.StreamIndex.HasValue)
                {
                    if (mediaInfo.VideoStreams.Length <= context.Requirement.StreamIndex)
                    {
                        context.Report(new MissingVideoStreamDiagnostic(
                            ShardName: context.Shard.Name,
                            ShardId: context.Shard.Id,
                            Variant: null,
                            StreamIndex: context.Requirement.StreamIndex.Value
                        ));
                        return ValueTask.CompletedTask;
                    }
                    CheckStream(
                        context,
                        context.Shard,
                        mediaInfo.VideoStreams[context.Requirement.StreamIndex.Value],
                        context.Requirement.StreamIndex.Value
                    );
                    return ValueTask.CompletedTask;
                }

                for (int streamIndex = 0; streamIndex < mediaInfo.VideoStreams.Length; ++streamIndex)
                {
                    CheckStream(
                        context,
                        context.Shard,
                        mediaInfo.VideoStreams[streamIndex],
                        streamIndex
                    );
                }
                break;

            case MediaBitrateKind.Audio:
                if (context.Requirement.StreamIndex.HasValue)
                {
                    if (mediaInfo.AudioStreams.Length <= context.Requirement.StreamIndex)
                    {
                        context.Report(new MissingAudioStreamDiagnostic(
                            ShardName: context.Shard.Name,
                            ShardId: context.Shard.Id,
                            Variant: null,
                            StreamIndex: context.Requirement.StreamIndex.Value
                        ));
                        return ValueTask.CompletedTask;
                    }
                    CheckStream(
                        context,
                        context.Shard,
                        mediaInfo.AudioStreams[context.Requirement.StreamIndex.Value],
                        context.Requirement.StreamIndex.Value
                    );
                    return ValueTask.CompletedTask;
                }

                for (int streamIndex = 0; streamIndex < mediaInfo.AudioStreams.Length; ++streamIndex)
                {
                    CheckStream(
                        context,
                        context.Shard,
                        mediaInfo.AudioStreams[streamIndex],
                        streamIndex
                    );
                }
                break;

            case MediaBitrateKind.Subtitles:
                if (context.Requirement.StreamIndex.HasValue)
                {
                    if (mediaInfo.SubtitleStreams.Length <= context.Requirement.StreamIndex)
                    {
                        context.Report(new MissingSubtitleStreamDiagnostic(
                            ShardName: context.Shard.Name,
                            ShardId: context.Shard.Id,
                            Variant: null,
                            StreamIndex: context.Requirement.StreamIndex.Value
                        ));
                        return ValueTask.CompletedTask;
                    }
                    CheckStream(
                        context,
                        context.Shard,
                        mediaInfo.SubtitleStreams[context.Requirement.StreamIndex.Value],
                        context.Requirement.StreamIndex.Value
                    );
                    return ValueTask.CompletedTask;
                }

                for (int streamIndex = 0; streamIndex < mediaInfo.SubtitleStreams.Length; ++streamIndex)
                {
                    CheckStream(
                        context,
                        context.Shard,
                        mediaInfo.SubtitleStreams[streamIndex],
                        streamIndex
                    );
                }
                break;
        }

        return ValueTask.CompletedTask;
    }

    private static void CheckStream<T>(
        IRequirementContext<MediaBitrateRequirement> context,
        IShard shard,
        T mediaStream,
        int streamIndex
    )
    {
        var (bitrate, tooLow, tooHigh) = mediaStream switch
        {
            VideoStreamInfo v =>
            (
                v.Bitrate,
                new Lazy<object>(() => new VideoBitrateTooLowDiagnostic(
                    ShardName: shard.Name,
                    ShardId: shard.Id,
                    Variant: null,
                    StreamIndex: streamIndex,
                    Min: context.Requirement.Min.GetValueOrDefault()
                )),
                new Lazy<object>(() => new VideoBitrateTooHighDiagnostic(
                    ShardName: shard.Name,
                    ShardId: shard.Id,
                    Variant: null,
                    StreamIndex: streamIndex,
                    Max: context.Requirement.Max.GetValueOrDefault()
                ))
            ),
            AudioStreamInfo a =>
            (
                a.Bitrate,
                new Lazy<object>(() => new AudioBitrateTooLowDiagnostic(
                    ShardName: shard.Name,
                    ShardId: shard.Id,
                    Variant: null,
                    StreamIndex: streamIndex,
                    Min: context.Requirement.Min.GetValueOrDefault()
                )),
                new Lazy<object>(() => new AudioBitrateTooHighDiagnostic(
                    ShardName: shard.Name,
                    ShardId: shard.Id,
                    Variant: null,
                    StreamIndex: streamIndex,
                    Max: context.Requirement.Max.GetValueOrDefault()
                ))
            ),
            SubtitleStreamInfo s =>
            (
                s.Bitrate,
                new Lazy<object>(() => new SubtitleBitrateTooLowDiagnostic(
                    ShardName: shard.Name,
                    ShardId: shard.Id,
                    Variant: null,
                    StreamIndex: streamIndex,
                    Min: context.Requirement.Min.GetValueOrDefault()
                )),
                new Lazy<object>(() => new SubtitleBitrateTooHighDiagnostic(
                    ShardName: shard.Name,
                    ShardId: shard.Id,
                    Variant: null,
                    StreamIndex: streamIndex,
                    Max: context.Requirement.Max.GetValueOrDefault()
                ))
            ),
            _ => throw new NotSupportedException($"Media stream type '{typeof(T)}' is not supported.")
        };

        if (context.Requirement.Min.HasValue && bitrate < context.Requirement.Min)
        {
            context.Report(tooLow.Value);
            return;
        }
        if (context.Requirement.Max.HasValue && bitrate > context.Requirement.Max)
        {
            context.Report(tooHigh.Value);
            return;
        }
    }
}
