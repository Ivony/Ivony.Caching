using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public class CacheServicePerformaceMonitor : ICacheServiceMonitor
  {
    public void OnCacheHitted( string cacheKey )
    {
      throw new NotImplementedException();
    }

    public void OnCacheMissed( string cacheKey )
    {
      throw new NotImplementedException();
    }
  }
}
