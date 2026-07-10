using System;
using System.Threading.Tasks;

namespace JokerNutrition.Business.Services;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    void Evict(string key);
    void EvictByPrefix(string prefix);
}
