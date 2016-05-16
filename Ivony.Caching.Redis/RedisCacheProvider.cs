using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching.Redis
{
  public class RedisCacheProvider : IAsyncCacheProvider
  {

    private readonly IDatabase _database;
    private readonly ITextSerializer _serializer;


    public RedisCacheProvider( IDatabase database, ITextSerializer serializer )
    {
      _database = database;
      _serializer = serializer;
    }


    public async Task Clear()
    {
      var connection = _database.Multiplexer;
      var endpoints = connection.GetEndPoints();

      foreach ( var item in endpoints )
      {
        var server = connection.GetServer( item );
        await server.FlushDatabaseAsync( _database.Database );
      }
    }

    public void Dispose()
    {
      _database.Multiplexer.Dispose();
    }

    public async Task<object> Get( string cacheKey )
    {
      return await _database.StringGetAsync( cacheKey );
    }

    public async Task Remove( string cacheKey )
    {
      await _database.KeyDeleteAsync( cacheKey );
    }

    public async Task Set( string cacheKey, object value, CachePolicyItem cachePolicy )
    {
      await _database.StringSetAsync( cacheKey, _serializer.Serialize( value ), cachePolicy.Expires - DateTime.UtcNow );
    }
  }
}
