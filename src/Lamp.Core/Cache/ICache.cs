using System.Collections.Generic;

namespace Lamp.Core.Cache
{
    public interface ICache<TKey, TValue>
    {
        void Set(TKey key, TValue value);

        bool TryGet(TKey key, out TValue value);

        bool ContainsKey(TKey key);

        ICollection<TKey> Keys { get; }

        ICollection<TValue> Values { get; }
    }
}
