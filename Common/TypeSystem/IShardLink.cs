namespace Kafe;

public interface IShardLink
{
    Hrib DestinationId { get; }
    KafeObject Payload { get; }
}
