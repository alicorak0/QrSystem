using Core.Utilities.IoC;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Core.CrossCuttingConcern.Caching.Microsoft
{
    public class MemoryCacheManager : ICacheManager
    {
        private IMemoryCache _memoryCache;

        // 🔥 KEY LISTESİ
        private static List<string> _keys = new List<string>();

        public MemoryCacheManager()
        {
            _memoryCache = ServiceTool.ServiceProvider.GetService<IMemoryCache>();
        }

        public void Add(string key, object value, int duration)
        {
            _memoryCache.Set(key, value, TimeSpan.FromMinutes(duration));

            if (!_keys.Contains(key))
                _keys.Add(key);
        }

        public T Get<T>(string key)
        {
            return _memoryCache.Get<T>(key);
        }

        public object Get(string key)
        {
            return _memoryCache.Get(key);
        }

        public bool IsAdd(string key)
        {
            return _memoryCache.TryGetValue(key, out _);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _keys.Remove(key);
        }

        // 🔥 BURASI FIX
        public void RemoveByPattern(string pattern)
        {
            var keysToRemove = _keys
                .Where(k => k.Contains(pattern))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keys.Remove(key);
            }
        }

        public void ClearAll()
        {
            foreach (var key in _keys)
            {
                _memoryCache.Remove(key);
            }

            _keys.Clear();
        }
    }
}