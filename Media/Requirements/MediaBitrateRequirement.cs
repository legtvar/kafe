using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaBitrateRequirement(
    int? Min,
    int? Max,
    MediaBitrateKind Kind,
    int? StreamIndex = null
) : IRequirement
{
    public static string Moniker => "bitrate";
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
                CheckAllStreams<VideoStreamInfo>(context, mediaInfo);
                break;

            case MediaBitrateKind.Audio:
                CheckAllStreams<AudioStreamInfo>(context, mediaInfo);
                break;

            case MediaBitrateKind.Subtitles:
                CheckAllStreams<SubtitleStreamInfo>(context, mediaInfo);
                break;
        }

        return ValueTask.CompletedTask;
    }

    private static void CheckAllStreams<T>(
        IShardRequirementContext<MediaBitrateRequirement> context,
        MediaInfo mediaInfo
    )
        where T : IMediaStreamInfo
    {
        IReadOnlyList<IMediaStreamInfo> streams = typeof(T) == typeof(VideoStreamInfo) ? mediaInfo.VideoStreams
            : typeof(T) == typeof(AudioStreamInfo) ? mediaInfo.AudioStreams
            : typeof(T) == typeof(SubtitleStreamInfo) ? mediaInfo.SubtitleStreams
            : throw new NotSupportedException($"Media stream type '{typeof(T)}' is not supported.");

        if (context.Requirement.StreamIndex.HasValue)
        {
            if (mediaInfo.VideoStreams.Length <= context.Requirement.StreamIndex)
            {
                context.Report(MediaConst.CreateMissingMediaStreamDiagnostic(
                    typeof(T),
                    context.Shard.Id,
                    context.Shard.Name,
                    null,
                    context.Requirement.StreamIndex.Value
                ));
                return;
            }
            CheckStream(
                context,
                (T)streams[context.Requirement.StreamIndex.Value],
                context.Requirement.StreamIndex.Value
            );
            return;
        }

        for (int streamIndex = 0; streamIndex < mediaInfo.VideoStreams.Length; ++streamIndex)
        {
            CheckStream(
                context,
                (T)streams[streamIndex],
                streamIndex
            );
        }
    }

    private static void CheckStream<T>(
        IShardRequirementContext<MediaBitrateRequirement> context,
        T mediaStream,
        int streamIndex
    )
        where T : IMediaStreamInfo
    {
        if (context.Requirement.Min.HasValue && mediaStream.Bitrate < context.Requirement.Min)
        {
            context.Report(CreateBitrateTooLowDiagnostic(
                context.Shard,
                null,
                mediaStream,
                streamIndex,
                context.Requirement.Min.Value
            ));
            return;
        }
        if (context.Requirement.Max.HasValue && mediaStream.Bitrate > context.Requirement.Max)
        {
            context.Report(CreateBitrateTooHighDiagnostic(
                context.Shard,
                null,
                mediaStream,
                streamIndex,
                context.Requirement.Max.Value
            ));
            return;
        }
    }

    // NB: I have opted NOT to add something like IBitrateTooLowDiagnostic because I intend to source-generate the
    //     diagnostic types from a JSON file later on.
    private static object CreateBitrateTooLowDiagnostic<T>(
        IShard shard,
        string? variant,
        T mediaStream,
        int streamIndex,
        long min
    )
        where T : IMediaStreamInfo
    {
        return mediaStream switch
        {
            VideoStreamInfo v => new VideoBitrateTooLowDiagnostic(
                shard.Name,
                shard.Id,
                variant,
                streamIndex,
                mediaStream.Bitrate,
                min
            ),
            AudioStreamInfo a => new AudioBitrateTooLowDiagnostic(
                shard.Name,
                shard.Id,
                variant,
                streamIndex,
                mediaStream.Bitrate,
                min
            ),
            SubtitleStreamInfo => new SubtitleBitrateTooLowDiagnostic(
                shard.Name,
                shard.Id,
                variant,
                streamIndex,
                mediaStream.Bitrate,
                min
            ),
            _ => throw new NotSupportedException($"Media stream type '{typeof(T)}' is not supported.")
        };
    }

    private static object CreateBitrateTooHighDiagnostic<T>(
        IShard shard,
        string? variant,
        T mediaStream,
        int streamIndex,
        long max
    )
        where T : IMediaStreamInfo
    {
        return mediaStream switch
        {
            VideoStreamInfo v => new VideoBitrateTooHighDiagnostic(
                shard.Name,
                shard.Id,
                variant,
                streamIndex,
                v.Bitrate,
                max
            ),
            AudioStreamInfo a => new AudioBitrateTooHighDiagnostic(
                shard.Name,
                shard.Id,
                variant,
                streamIndex,
                a.Bitrate,
                max
            ),
            SubtitleStreamInfo s => new SubtitleBitrateTooHighDiagnostic(
                shard.Name,
                shard.Id,
                variant,
                streamIndex,
                s.Bitrate,
                max
            ),
            _ => throw new NotSupportedException($"Media stream type '{typeof(T)}' is not supported.")
        };
    }
}
