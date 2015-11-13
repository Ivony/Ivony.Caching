using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public interface IAsyncCacheProvider
  {

    Task<object> Get( string key );

    Task Set( string key, object value, CachePolicy cachePolicy );

    Task Remove( string cacheKey );

    Task Clear();
  }
}
