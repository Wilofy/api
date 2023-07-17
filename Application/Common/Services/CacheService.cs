using Application.Common.Abstractions;
using StackExchange.Redis;
using System.Text.Json;

namespace Application.Common.Services;
public class CacheService : ICacheService
{
    private IDatabase _database;

    public CacheService()
    {
        var redis = ConnectionMultiplexer.Connect("localhost:6379");
        _database = redis.GetDatabase();
    }

    public T? GetData<T>(string key)
    {
        var value = _database.StringGet(key);

        if (!value.IsNullOrEmpty)
        {
            return JsonSerializer.Deserialize<T>(value!);
        }

        return default;
    }

    public object RemoveData(string key)
    {
        var keyExist = _database.KeyExists(key);

        return keyExist ? _database.KeyDelete(key) : false;

    }

    public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
    {
        var expirTime = expirationTime.DateTime.Subtract(DateTime.Now);
        var isSet = _database.StringSet(key, JsonSerializer.Serialize(value), expirTime);
        return isSet;
    }

    public bool SetData<T>(string key, T value)
    {
        var isSet = _database.StringSet(key, JsonSerializer.Serialize(value));
        return isSet;
    }
}

