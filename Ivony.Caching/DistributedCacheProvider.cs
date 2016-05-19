using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{


  /// <summary>
  /// 协助实现一个分布式缓存提供程序，分布式缓存提供程序可以将缓存按照缓存键保存到不同的缓存提供程序去
  /// </summary>
  public abstract class DistributedCacheProvider : IAsyncCacheProvider
  {



    /// <summary>
    /// 派生类实现此方法根据缓存键获取缓存提供程序
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <returns></returns>
    protected abstract Task<IAsyncCacheProvider> GetCacheProvider( string cacheKey );

    /// <summary>
    /// 派生类实现此方法获取所有缓存提供程序
    /// </summary>
    /// <returns>所有缓存提供程序</returns>
    protected abstract IAsyncCacheProvider[] GetAllCacheProviders();


    async Task IAsyncCacheProvider.Clear()
    {
      var providers = GetAllCacheProviders();
      await Task.WhenAll( providers.Select( item => item.Clear() ).ToArray() );

    }

    /// <summary>
    /// 释放所有资源
    /// </summary>
    public virtual void Dispose()
    {

      foreach ( var item in GetAllCacheProviders() )
      {
        item.Dispose();
      }
    }

    async Task<object> IAsyncCacheProvider.Get( string cacheKey )
    {
      var provider = await GetCacheProvider( cacheKey );
      return provider.Get( cacheKey );

    }

    async Task IAsyncCacheProvider.Remove( string cacheKey )
    {
      var provider = await GetCacheProvider( cacheKey );
      await provider.Remove( cacheKey );
    }

    async Task IAsyncCacheProvider.Set( string cacheKey, object value, CachePolicyItem cachePolicy )
    {
      var provider = await GetCacheProvider( cacheKey );
      await provider.Set( cacheKey, value, cachePolicy );
    }
  }
}
