using System;
using System.Dynamic;

namespace WebApplication.services
{
    public interface IRedisCacheService
    {
        string GetInfo(string key);
        void SetInfo(string key, string value);
    }
}