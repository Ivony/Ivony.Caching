using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public interface IAsyncL2CacheProvider : IAsyncCacheProvider
  {

    Task<object> Get( string key, out CachePolicy cachePolicy );
  }
}
