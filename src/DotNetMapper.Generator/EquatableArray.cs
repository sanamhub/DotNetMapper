using System.Collections.Immutable;

namespace DotNetMapper.Generator;

// A value-equatable wrapper so MapCallModel can be a record with reference-free equality.
// Incremental generator caching depends on value equality; ImmutableArray<T> alone does not
// override Equals, so a record field of that type would compare by reference and never cache.
internal readonly struct EquatableArray<T> : IEquatable<EquatableArray<T>>
{
    private readonly ImmutableArray<T> _items;

    public EquatableArray(T[] items)
    {
        _items = ImmutableArray.Create(items);
    }

    public int Length => _items.IsDefault ? 0 : _items.Length;

    public T this[int index] => _items[index];

    public bool Equals(EquatableArray<T> other)
    {
        if (_items.IsDefault != other._items.IsDefault)
        {
            return false;
        }

        if (_items.IsDefault)
        {
            return true;
        }

        if (_items.Length != other._items.Length)
        {
            return false;
        }

        for (int i = 0; i < _items.Length; i++)
        {
            if (!EqualityComparer<T>.Default.Equals(_items[i], other._items[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var item in _items)
            {
                hash = (hash * 31) + (item is null ? 0 : EqualityComparer<T>.Default.GetHashCode(item));
            }

            return hash;
        }
    }
}
