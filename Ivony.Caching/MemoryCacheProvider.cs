using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  public class MemoryCacheProvider : ICacheProvider
  {

    private MemoryCache _host;

    private string _name;

    public MemoryCacheProvider( string name )
    {
      _name = name;
      _host = new MemoryCache( name );
    }

    public void Clear()
    {
      _host = new MemoryCache( _name );
    }

    public object Get( string key )
    {
      return _host.Get( key );
    }

    public void Remove( string cacheKey )
    {
      _host.Remove( cacheKey );
    }

    public void Set( string key, object value, CachePolicy cachePolicy )
    {
      _host.Set( new System.Runtime.Caching.CacheItem( key, value ), CreateCacheItemPolicy( cachePolicy ) );
    }

    private CacheItemPolicy CreateCacheItemPolicy( CachePolicy cachePolicy )
    {
      return new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.AddHours( 1 ) };
    }
  }
}
