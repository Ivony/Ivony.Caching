using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{


  /// <summary>
  /// 定义一个缓存策略提供程序
  /// </summary>
  public interface ICachePolicyProvider
  {

    /// <summary>
    /// 创建一个缓存策略
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="cacheValue">缓存值</param>
    /// <returns>缓存策略</returns>
    CachePolicy CreateCachePolicy( string cacheKey, object cacheValue );

  }



  /// <summary>
  /// 提供一些预设的缓存策略提供程序
  /// </summary>
  public static class CachePolicyProviders
  {

    /// <summary>
    /// 获取一个按照指定时间后过期的缓存策略提供程序
    /// </summary>
    /// <param name="expireTime">过期时间（在获取缓存策略多久后过期）</param>
    /// <returns>缓存策略提供程序</returns>
    public static ICachePolicyProvider Expires( TimeSpan expireTime )
    {
      return new ExpireTimeCachePolicyProvider( expireTime );
    }


    private sealed class ExpireTimeCachePolicyProvider : ICachePolicyProvider
    {

      private TimeSpan _expireTime;

      public ExpireTimeCachePolicyProvider( TimeSpan expireTime )
      {
        if ( expireTime <= TimeSpan.Zero )
          throw new ArgumentOutOfRangeException( nameof( expireTime ) );


        _expireTime = expireTime;
      }

      public CachePolicy CreateCachePolicy( string cacheKey, object cacheValue )
      {
        return CachePolicy.Expired( _expireTime );
      }
    }



    /// <summary>
    /// 设置缓存策略提供程序的默认缓存优先级
    /// </summary>
    /// <param name="provider">要设置的缓存策略提供程序</param>
    /// <param name="priority">默认缓存优先级</param>
    /// <returns>应用了默认缓存优先级的缓存策略提供程序</returns>
    public static ICachePolicyProvider SetPriority( this ICachePolicyProvider provider, CachePriority priority )
    {
      return new CachePolicyProviderWithPriority( provider, priority );
    }

    private class CachePolicyProviderWithPriority : ICachePolicyProvider
    {
      private CachePriority _priority;
      private ICachePolicyProvider _provider;

      public CachePolicyProviderWithPriority( ICachePolicyProvider provider, CachePriority priority )
      {
        _provider = provider;
        _priority = priority;
      }

      public CachePolicy CreateCachePolicy( string cacheKey, object cacheValue )
      {
        return _provider.CreateCachePolicy( cacheKey, cacheValue ).SetPriority( _priority );
      }
    }
  }

}
