using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace Chowbi_Utilities.Helpers
{
    public class CHWCache
    {
        ICacheStorage _Storage;
        public CHWCache(ICacheStorage? storage = null)
        {
            _Storage = storage ?? new DefaultCacheStorage();
        }
        public Task<T?> GetOrSet<T>(IConvertible key
            , Func<Task<T>> loadObject
            , Func<T, DateTime?> getCachedDate
            , Func<Task<DateTime?>> getSourceDate
            , int checkIntervalSec = 20
            , int checkIntervalIfNullCoeff = 6)
                => _Storage.GetOrSet(key, loadObject, getCachedDate, getSourceDate, checkIntervalSec, checkIntervalIfNullCoeff);
        public Task<T?> Get<T>(IConvertible key) => _Storage.Get<T>(key);
        public void Remove(IConvertible key) => _Storage.Remove(key);
    }

    public interface ICacheStorage
    {
        public Task<T?> GetOrSet<T>(IConvertible key
            , Func<Task<T>> loadObject
            , Func<T, DateTime?> getCachedDate
            , Func<Task<DateTime?>> getSourceDate
            , int checkIntervalSec = 20
            , int checkIntervalIfNullCoeff = 6);
        public Task<T?> Get<T>(IConvertible key);

        public void Remove(IConvertible key);
    }
    class DefaultCacheStorage : ICacheStorage
    {
        static MemoryCache _Cache = new MemoryCache(new MemoryCacheOptions());

        public async Task<T?> Get<T>(IConvertible key) => await (((CachedItem<T>)_Cache.Get(key))?.GetValue() ?? Task.FromResult<T?>(default));

        public async Task<T?> GetOrSet<T>(IConvertible key
            , Func<Task<T>> loadObject
            , Func<T, DateTime?> getCachedDate
            , Func<Task<DateTime?>> getSourceDate
            , int checkIntervalSec = 20
            , int checkIntervalIfNullCoeff = 6)
        {
            if (!_Cache.TryGetValue(key, out object value))
            {
                value = new CachedItem<T>(key, loadObject, getCachedDate, getSourceDate, checkIntervalSec, checkIntervalIfNullCoeff);
                _Cache.Set(key, value, TimeSpan.FromHours(6));
            }
            return await ((CachedItem<T>)value).GetValue();
        }

        public void Remove(IConvertible key) => _Cache.Remove(key);
    }

    class CachedItem<T>
    {
        public int CheckIntervalSec { get; set; }
        public int CheckIntervalIfNullCoeff { get; set; }

        public IConvertible Key { get; private set; }
        public T? CurrentValue { get; private set; }
        public DateTime? CheckedDate { get; private set; }

        Func<Task<T>> _LoadObject { get; set; }
        bool _Loaded { get; set; }
        Func<T, DateTime?> _GetCachedDate { get; set; }
        Func<Task<DateTime?>> _GetSourceDate { get; set; }

        public CachedItem(IConvertible key
            , Func<Task<T>> loadObject
            , Func<T, DateTime?> getCachedDate
            , Func<Task<DateTime?>> getSourceDate
            , int checkIntervalSec = 20
            , int checkIntervalIfNullCoeff = 6)
        {
            Key = key;
            _LoadObject = loadObject;
            _GetCachedDate = getCachedDate;
            _GetSourceDate = getSourceDate;
            CheckIntervalSec = checkIntervalSec;
            CheckIntervalIfNullCoeff = checkIntervalIfNullCoeff;
        }

        public async Task<T?> GetValue()
        {
            int interval;
            if (_Loaded && CurrentValue == null)
                interval = CheckIntervalIfNullCoeff * CheckIntervalSec;
            else
                interval = CheckIntervalSec;

            if (CheckedDate.GetValueOrDefault().AddSeconds(interval) > DateTime.Now)
                return CurrentValue;

            if (_Loaded && CurrentValue != null)
            {
                DateTime? source = await _GetSourceDate();
                DateTime? cache = _GetCachedDate(CurrentValue);
                if (source == null) if (Nullable.GetUnderlyingType(typeof(T)) == null)
                        throw new Exception("Can't have a null source date updated if the generic type is not nullable");
                    else
                    {
                        CurrentValue = default;
                        return CurrentValue;
                    }
                else if (cache.HasValue && source > cache)
                    return CurrentValue;
            }

            CurrentValue = await Task.Run(_LoadObject);
            _Loaded = true;

            return CurrentValue;
        }
    }
}
