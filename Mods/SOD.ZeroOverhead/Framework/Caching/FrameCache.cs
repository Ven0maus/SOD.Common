using SOD.ZeroOverhead.Framework.Pooling;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.ZeroOverhead.Framework.Caching
{
    /// <summary>
    /// Simple frame-based cache, cleans invalid records on store, and removes invalid records on access.
    /// </summary>
    public class FrameCache<TKey, TValue>
        where TKey : struct, IEquatable<TKey>
    {
        private readonly Dictionary<TKey, CacheEntry<TValue>> _cache = new();
        private readonly int _ttlFrames;
        private readonly int _cleanupInterval;

        private int _lastCleanupFrame;

        public FrameCache(int ttlFrames, int cleanupIntervalFrames = 120)
        {
            _ttlFrames = ttlFrames;
            _cleanupInterval = cleanupIntervalFrames;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var entry) &&
                Time.frameCount - entry.LastFrame <= _ttlFrames)
            {
                value = entry.Value;
                return true;
            }
            else if (entry != null)
            {
                _cache.Remove(key);
            }

            value = default;
            return false;
        }

        public void Store(TKey key, TValue value)
        {
            _cache[key] = new CacheEntry<TValue>
            {
                Value = value,
                LastFrame = Time.frameCount
            };

            CleanupIfNeeded();
        }

        public void Invalidate(TKey key)
        {
            _cache.Remove(key);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        private void CleanupIfNeeded()
        {
            if (Time.frameCount - _lastCleanupFrame < _cleanupInterval)
                return;

            _lastCleanupFrame = Time.frameCount;
            int frame = Time.frameCount;

            var toRemove = SimpleListPool<TKey>.Get();

            foreach (var kv in _cache)
            {
                if (frame - kv.Value.LastFrame > _ttlFrames)
                    toRemove.Add(kv.Key);
            }

            foreach (var key in toRemove)
                _cache.Remove(key);

            SimpleListPool<TKey>.Release(toRemove);
        }

        private sealed class CacheEntry<T>
        {
            public T Value;
            public int LastFrame;
        }
    }
}
