using Kafe.Data.Events;
using Kafe.Media;
using Marten.Events;
using Marten.Events.Aggregation;
using System.Collections.Immutable;

namespace Kafe.Data.Aggregates;

public abstract record ArtifactBase(
    string Id,
    ArtifactKind Kind,
    CreationMethod CreationMethod,
    LocalizedString Name) : IEntity;

public record FilmArtifact(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name,
    ImmutableArray<MediaInfo> Media)
     : ArtifactBase(Id, ArtifactKind.Film, CreationMethod, Name);

public record ImageArtifact(
    string Id,
    CreationMethod CreationMethod,
    LocalizedString Name)
    : ArtifactBase(Id, ArtifactKind.Image, CreationMethod, Name);

public class ArtifactProjections : SingleStreamAggregation<FilmArtifact>
{
    public ArtifactProjections()
    {
    }

    public FilmArtifact Create(IEvent<FilmArtifactCreated> e)
    {
        return new FilmArtifact(
            Id: e.StreamKey!,
            CreationMethod: e.Data.CreationMethod,
            Name: e.Data.Name,
            Variants: ImmutableArray.Create<VideoQualityPreset>(VideoQualityPreset.Original),
            Media: );

        return e.Data.Kind switch
        {
            ArtifactKind.Film => 
        };
        return new Artifact(
            Id: e.StreamKey!,
            Kind: e.Data.Kind,
            CreationMethod: e.Data.CreationMethod,
            Name: e.Data.Name,
            FileName: e.Data.FileName,
            Metadata: e.Data.Metadata);
    }

    public Artifact Apply(ArtifactInfoChanged e, Artifact v)
    {
        return v with
        {
            Name = e.Name ?? v.Name,
            FileName = e.FileName ?? v.FileName,
            Metadata = e.Metadata ?? v.Metadata
        };
    }
}
