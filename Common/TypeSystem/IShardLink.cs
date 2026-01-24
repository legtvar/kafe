namespace Kafe;

public interface IShardLink
{
    Hrib Id { get; }
    KafeObject Payload { get; }
}
