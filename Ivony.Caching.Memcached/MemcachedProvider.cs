using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching.Memcached
{
  public class MemcachedProvider : ICacheProvider
  {

    MemcachedClient client;

    public MemcachedProvider( MemcachedClientConfiguration configuration ) : this( new MemcachedClient( configuration ) ) { }


    public MemcachedProvider( MemcachedClient client )
    {
      this.client = client;
    }

    void ICacheProvider.Clear()
    {
      client.FlushAll();
    }

    object ICacheProvider.Get( string key )
    {
      return client.Get( key );
    }

    void ICacheProvider.Remove( string cacheKey )
    {
      client.Remove( cacheKey );
    }

    void ICacheProvider.Set( string key, object value, CachePolicy cachePolicy )
    {
      var result = client.ExecuteStore( StoreMode.Set, key, value, cachePolicy.Expires );

      if ( result.Success == false )
        throw new Exception( result.Message, result.Exception );

    }

    public void Dispose()
    {
      client.Dispose();
    }
  }
}
