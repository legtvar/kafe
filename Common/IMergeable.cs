namespace Kafe;

public interface IMergeable<T>
{
    object MergeWith(T other);
}
