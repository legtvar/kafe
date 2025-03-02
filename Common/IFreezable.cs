namespace Kafe;

public interface IFreezable
{
    bool IsFrozen { get; }

    void Freeze();
}
