using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace NoSQLAPI.Controllers
{
    [RoutePrefix("api/redis")]
    public class RedisController : ApiController
    {
        private static Lazy<ConnectionMultiplexer> _lazyConnection;
        private readonly IDatabase _cache;

        private static ConnectionMultiplexer cacheConnection
        {
            get
            {
                return _lazyConnection.Value;
            }
        }

        static RedisController()
        {
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                return ConnectionMultiplexer.Connect(ConfigurationManager.ConnectionStrings["RedisConnection"].ConnectionString);
            });
        }

        public RedisController()
        {
            _cache = cacheConnection.GetDatabase();
        }


        [HttpPost, Route("key/{key}/value/{value}")]
        public async Task<IHttpActionResult> SetValue(string key, string value)
        {
            await _cache.StringSetAsync(key, value, new TimeSpan(0, 0, 10), When.NotExists, CommandFlags.FireAndForget);
            return Ok();
        }

        [HttpGet, Route("retrieve/{key}")]
        public async Task<IHttpActionResult> GetValue(string key)
        {
            var value = await _cache.StringGetAsync(key);
            if (value.HasValue)
                return Ok(value);
            else
                return Ok("key not found");
        }
    }
}
