#pragma warning disable 0618

using System;
using Kafe.Media;
using Marten.Services.Json.Transformations;

namespace Kafe.Data.Events
{
    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IShardEvent
    {
        [Hrib]
        string ShardId { get; }
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IShardCreated : IShardEvent
    {
        CreationMethod CreationMethod { get; }

        [Hrib]
        string ArtifactId { get; }
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IShardVariantAdded : IShardEvent
    {
        string Name { get; }
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IShardVariantRemoved : IShardEvent
    {
        string Name { get; }
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface ISubtitlesShardEvent : IShardEvent
    {
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public record SubtitlesShardCreated(
        [Hrib] string ShardId,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        SubtitlesInfo OriginalVariantInfo
    ) : ISubtitlesShardEvent, IShardCreated;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record SubtitlesShardVariantsAdded(
        [Hrib] string ShardId,
        string Name,
        SubtitlesInfo Info
    ) : ISubtitlesShardEvent, IShardVariantAdded;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record SubtitlesShardVariantsRemoved(
        [Hrib] string ShardId,
        string Name
    ) : ISubtitlesShardEvent, IShardVariantRemoved;

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IVideoShardEvent : IShardEvent
    {
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public record VideoShardCreated(
        [Hrib] string ShardId,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        MediaInfo OriginalVariantInfo
    ) : IVideoShardEvent, IShardCreated;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record VideoShardVariantAdded(
        [Hrib] string ShardId,
        string Name,
        MediaInfo Info
    ) : IVideoShardEvent, IShardVariantAdded;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record VideoShardVariantRemoved(
        [Hrib] string ShardId,
        string Name
    ) : IVideoShardEvent, IShardVariantRemoved;

    [Obsolete("Use the new shard universal shard events instead.")]
    public interface IImageShardEvent : IShardEvent
    {
    }

    [Obsolete("Use the new shard universal shard events instead.")]
    public record ImageShardCreated(
        [Hrib] string ShardId,
        CreationMethod CreationMethod,
        [Hrib] string ArtifactId,
        ImageInfo OriginalVariantInfo
    ) : IImageShardEvent, IShardCreated;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record ImageShardVariantsAdded(
        [Hrib] string ShardId,
        string Name,
        ImageInfo Info
    ) : IImageShardEvent, IShardVariantAdded;

    [Obsolete("Use the new shard universal shard events instead.")]
    public record ImageShardVariantsRemoved(
        [Hrib] string ShardId,
        string Name
    ) : IImageShardEvent, IShardVariantRemoved;
}

namespace Kafe.Data.Events.Upcasts
{
    internal class VideoShardCreatedUpcaster : EventUpcaster<VideoShardCreated, ShardCreated>
    {
        private readonly KafeObjectFactory factory;

        public VideoShardCreatedUpcaster(KafeObjectFactory factory)
        {
            this.factory = factory;
        }

        protected override ShardCreated Upcast(VideoShardCreated oldEvent)
        {
            return new ShardCreated(
                ShardId: oldEvent.ShardId,
                CreationMethod: oldEvent.CreationMethod,
                ArtifactId: oldEvent.ArtifactId,
                Size: oldEvent.OriginalVariantInfo.FileLength,
                Filename: null,
                Metadata: factory.Wrap(oldEvent.OriginalVariantInfo)
            );
        }
    }

    internal class VideoShardVariantAddedUpcaster : EventUpcaster<VideoShardVariantAdded, ShardVariantAdded>
    {
        private readonly KafeObjectFactory factory;

        public VideoShardVariantAddedUpcaster(KafeObjectFactory factory)
        {
            this.factory = factory;
        }

        protected override ShardVariantAdded Upcast(VideoShardVariantAdded oldEvent)
        {
            return new ShardVariantAdded(
                ShardId: oldEvent.ShardId,
                Name: oldEvent.Name,
                ExistingValueHandling: ExistingKafeObjectHandling.MergeOrKeep,
                Metadata: factory.Wrap(oldEvent.Info)
            );
        }
    }

    internal class VideoShardVariantRemovedUpcaster : EventUpcaster<VideoShardVariantRemoved, ShardVariantRemoved>
    {
        protected override ShardVariantRemoved Upcast(VideoShardVariantRemoved oldEvent)
        {
            return new ShardVariantRemoved(
                ShardId: oldEvent.ShardId,
                Name: oldEvent.Name
            );
        }
    }

    internal class ImageShardCreatedUpcaster : EventUpcaster<ImageShardCreated, ShardCreated>
    {
        private readonly KafeObjectFactory factory;

        public ImageShardCreatedUpcaster(KafeObjectFactory factory)
        {
            this.factory = factory;
        }

        protected override ShardCreated Upcast(ImageShardCreated oldEvent)
        {
            return new ShardCreated(
                ShardId: oldEvent.ShardId,
                CreationMethod: oldEvent.CreationMethod,
                ArtifactId: oldEvent.ArtifactId,
                Size: null,
                Filename: null,
                Metadata: factory.Wrap(oldEvent.OriginalVariantInfo)
            );
        }
    }

    internal class ImageShardVariantsAddedUpcaster : EventUpcaster<ImageShardVariantsAdded, ShardVariantAdded>
    {
        private readonly KafeObjectFactory factory;

        public ImageShardVariantsAddedUpcaster(KafeObjectFactory factory)
        {
            this.factory = factory;
        }

        protected override ShardVariantAdded Upcast(ImageShardVariantsAdded oldEvent)
        {
            return new ShardVariantAdded(
                ShardId: oldEvent.ShardId,
                Name: oldEvent.Name,
                ExistingValueHandling: ExistingKafeObjectHandling.MergeOrKeep,
                Metadata: factory.Wrap(oldEvent.Info)
            );
        }
    }

    internal class ImageShardVariantsRemovedUpcaster : EventUpcaster<ImageShardVariantsRemoved, ShardVariantRemoved>
    {
        protected override ShardVariantRemoved Upcast(ImageShardVariantsRemoved oldEvent)
        {
            return new ShardVariantRemoved(
                ShardId: oldEvent.ShardId,
                Name: oldEvent.Name
            );
        }
    }

    internal class SubtitlesShardCreatedUpcaster : EventUpcaster<SubtitlesShardCreated, ShardCreated>
    {
        private readonly KafeObjectFactory factory;

        public SubtitlesShardCreatedUpcaster(KafeObjectFactory factory)
        {
            this.factory = factory;
        }

        protected override ShardCreated Upcast(SubtitlesShardCreated oldEvent)
        {
            return new ShardCreated(
                ShardId: oldEvent.ShardId,
                CreationMethod: oldEvent.CreationMethod,
                ArtifactId: oldEvent.ArtifactId,
                Size: null,
                Filename: null,
                Metadata: factory.Wrap(oldEvent.OriginalVariantInfo)
            );
        }
    }

    internal class SubtitlesShardVariantsAddedUpcaster : EventUpcaster<SubtitlesShardVariantsAdded, ShardVariantAdded>
    {
        private readonly KafeObjectFactory factory;

        public SubtitlesShardVariantsAddedUpcaster(KafeObjectFactory factory)
        {
            this.factory = factory;
        }

        protected override ShardVariantAdded Upcast(SubtitlesShardVariantsAdded oldEvent)
        {
            return new ShardVariantAdded(
                ShardId: oldEvent.ShardId,
                Name: oldEvent.Name,
                ExistingValueHandling: ExistingKafeObjectHandling.MergeOrKeep,
                Metadata: factory.Wrap(oldEvent.Info)
            );
        }
    }

    internal class SubtitlesShardVariantsRemovedUpcaster
        : EventUpcaster<SubtitlesShardVariantsRemoved, ShardVariantRemoved>
    {
        protected override ShardVariantRemoved Upcast(SubtitlesShardVariantsRemoved oldEvent)
        {
            return new ShardVariantRemoved(
                ShardId: oldEvent.ShardId,
                Name: oldEvent.Name
            );
        }
    }
}
