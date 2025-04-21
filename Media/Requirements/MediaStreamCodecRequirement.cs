using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Kafe.Core.Requirements;
using Kafe.Media.Diagnostics;

namespace Kafe.Media.Requirements;

public record MediaStreamCodecRequirement(
    ImmutableArray<string> Include,
    ImmutableArray<string> Exclude,
    MediaStreamKind Kind,
    int? StreamIndex
) : IRequirement
{
    public static string Moniker { get; } = "media-stream-codec";
}

public class MediaStreamCodecRequirementHandler : ShardRequirementHandlerBase<MediaStreamCodecRequirement>
{
    public override ValueTask Handle(IShardRequirementContext<MediaStreamCodecRequirement> context)
    {
        var mediaInfo = context.RequireMediaInfo();
        if (mediaInfo is null)
        {
            return ValueTask.CompletedTask;
        }

        var allowedCodecs = context.Requirement.Include.Except(context.Requirement.Exclude).ToImmutableArray().Sort();

        switch (context.Requirement.Kind)
        {
            case MediaStreamKind.Video:
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
                        mediaInfo.VideoStreams[context.Requirement.StreamIndex.Value],
                        context.Requirement.StreamIndex.Value,
                        allowedCodecs
                    );
                    return ValueTask.CompletedTask;
                }

                for (int streamIndex = 0; streamIndex < mediaInfo.VideoStreams.Length; ++streamIndex)
                {
                    CheckStream(
                        context,
                        mediaInfo.VideoStreams[streamIndex],
                        streamIndex,
                        allowedCodecs
                    );
                }
                break;

            case MediaStreamKind.Audio:
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
                        mediaInfo.AudioStreams[context.Requirement.StreamIndex.Value],
                        context.Requirement.StreamIndex.Value,
                        allowedCodecs
                    );
                    return ValueTask.CompletedTask;
                }

                for (int streamIndex = 0; streamIndex < mediaInfo.AudioStreams.Length; ++streamIndex)
                {
                    CheckStream(
                        context,
                        mediaInfo.AudioStreams[streamIndex],
                        streamIndex,
                        allowedCodecs
                    );
                }
                break;

            case MediaStreamKind.Subtitles:
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
                        mediaInfo.SubtitleStreams[context.Requirement.StreamIndex.Value],
                        context.Requirement.StreamIndex.Value,
                        allowedCodecs
                    );
                    return ValueTask.CompletedTask;
                }

                for (int streamIndex = 0; streamIndex < mediaInfo.SubtitleStreams.Length; ++streamIndex)
                {
                    CheckStream(
                        context,
                        mediaInfo.SubtitleStreams[streamIndex],
                        streamIndex,
                        allowedCodecs
                    );
                }
                break;
        }

        return ValueTask.CompletedTask;
    }

    private static void CheckStream<T>(
        IShardRequirementContext<MediaStreamCodecRequirement> context,
        T mediaStream,
        int streamIndex,
        ImmutableArray<string> allowedCodecs
    )
    {
        var (codec, badCodec) = mediaStream switch
        {
            VideoStreamInfo v =>
            (
                v.Codec,
                new Lazy<object>(() => new VideoCodecNotAllowedDiagnostic(
                    context.Shard.Id,
                    context.Shard.Name,
                    v.Codec,
                    allowedCodecs,
                    streamIndex
                ))
            ),
            AudioStreamInfo a =>
            (
                a.Codec,
                new Lazy<object>(() => new AudioCodecNotAllowedDiagnostic(
                    context.Shard.Id,
                    context.Shard.Name,
                    a.Codec,
                    allowedCodecs,
                    streamIndex
                ))
            ),
            SubtitleStreamInfo s =>
            (
                s.Codec,
                new Lazy<object>(() => new SubtitleCodecNotAllowedDiagnostic(
                    context.Shard.Id,
                    context.Shard.Name,
                    s.Codec,
                    allowedCodecs,
                    streamIndex
                ))
            ),
            _ => throw new NotSupportedException($"Media stream type '{typeof(T)}' is not supported.")
        };

        if (!allowedCodecs.Contains(codec))
        {
            context.Report(badCodec.Value);
        }
    }
}
