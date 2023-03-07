using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Kafe;

public sealed partial class LocalizedString : IDictionary<string, string>
{
    string IDictionary<string, string>.this[string key] { get => ((IDictionary<string, string>)data)[key]; set => ((IDictionary<string, string>)data)[key] = value; }

    ICollection<string> IDictionary<string, string>.Keys => ((IDictionary<string, string>)data).Keys;

    public ICollection<string> Values => ((IDictionary<string, string>)data).Values;

    int ICollection<KeyValuePair<string, string>>.Count => data.Count;

    bool ICollection<KeyValuePair<string, string>>.IsReadOnly => ((ICollection<KeyValuePair<string, string>>)data).IsReadOnly;

    void IDictionary<string, string>.Add(string key, string value)
    {
        ((IDictionary<string, string>)data).Add(key, value);
    }

    void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
    {
        ((ICollection<KeyValuePair<string, string>>)data).Add(item);
    }

    void ICollection<KeyValuePair<string, string>>.Clear()
    {
        ((ICollection<KeyValuePair<string, string>>)data).Clear();
    }

    bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
    {
        return data.Contains(item);
    }

    bool IDictionary<string, string>.ContainsKey(string key)
    {
        return data.ContainsKey(key);
    }

    void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, string>>)data).CopyTo(array, arrayIndex);
    }

    IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, string>>)data).GetEnumerator();
    }

    bool IDictionary<string, string>.Remove(string key)
    {
        return ((IDictionary<string, string>)data).Remove(key);
    }

    bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
    {
        return ((ICollection<KeyValuePair<string, string>>)data).Remove(item);
    }

    bool IDictionary<string, string>.TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        return data.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)data).GetEnumerator();
    }
}
