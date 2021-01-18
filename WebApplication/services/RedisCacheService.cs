using StackExchange.Redis;

namespace WebApplication.services
{
    public class RedisCacheService : IRedisCacheService
    {

        private readonly IDatabase _db;
        
        public RedisCacheService()
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("127.0.0.1:6379");
            _db = redis.GetDatabase();
        }

        public string GetInfo(string key)
        {
            if (_db.KeyExists(key))
            {
                return _db.StringGet(key);
            }
            return null;
        }

        public void SetInfo(string key, string value)
        {
            _db.StringSet(key, value);
        }

    }
}