using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public static class CacheExtensions
  {

    /// <summary>
    /// 将一个同步缓存提供程序转换为异步缓存提供程序
    /// </summary>
    /// <param name="cacheProvider"></param>
    /// <returns></returns>
    public static IAsyncCacheProvider AsAsyncProvider( this ICacheProvider cacheProvider )
    {
      return new AsyncCacheProvider( cacheProvider );
    }

    private class AsyncCacheProvider : IAsyncCacheProvider
    {
      private ICacheProvider provider;

      public AsyncCacheProvider( ICacheProvider cacheProvider )
      {
        provider = cacheProvider;
      }

      public Task Clear()
      {
        provider.Clear();
        return Task.CompletedTask;
      }

      public Task<object> Get( string key )
      {
        return Task.FromResult( provider.Get( key ) );
      }

      public Task Remove( string cacheKey )
      {
        provider.Remove( cacheKey );
        return Task.CompletedTask;
      }

      public Task Set( string key, object value, CachePolicy cachePolicy )
      {
        provider.Set( key, value, cachePolicy );
        return Task.CompletedTask;
      }
    }
  }
}
