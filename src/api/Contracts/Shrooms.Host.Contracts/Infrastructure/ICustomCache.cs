﻿namespace Shrooms.Host.Contracts.Infrastructure
{
    public interface ICustomCache<TKey, TValue>
    {
        bool TryAdd(TKey key, TValue value);
        bool TryRemoveEntry(TKey key);
        bool TryGetValue(TKey key, out TValue value);
        void Clear();
    }
}
