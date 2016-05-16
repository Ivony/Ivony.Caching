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
  public abstract class CachePolicy
  {


    /// <summary>
    /// 创建一个缓存策略
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="cacheValue">缓存值</param>
    /// <returns>缓存策略</returns>
    public abstract CachePolicyItem CreatePolicyItem( string cacheKey, object cacheValue );



    /// <summary>
    /// 获取一个按照指定时间后过期的缓存策略提供程序
    /// </summary>
    /// <param name="expireTime">过期时间（在获取缓存策略多久后过期）</param>
    /// <returns>缓存策略提供程序</returns>
    public static CachePolicy Expires( TimeSpan expireTime )
    {
      return new ExpireTimeCachePolicyProvider( expireTime );
    }


    private sealed class ExpireTimeCachePolicyProvider : CachePolicy
    {

      private TimeSpan _expireTime;

      public ExpireTimeCachePolicyProvider( TimeSpan expireTime )
      {
        if ( expireTime <= TimeSpan.Zero )
          throw new ArgumentOutOfRangeException( nameof( expireTime ) );


        _expireTime = expireTime;
      }

      public override CachePolicyItem CreatePolicyItem( string cacheKey, object cacheValue )
      {
        return new CachePolicyItem( DateTime.UtcNow + _expireTime + Jitter( _expireTime ) );
      }


      private Random random = new Random( DateTime.Now.Millisecond );


      /// <summary>
      /// 获取时间抖动
      /// </summary>
      /// <param name="time">失效时间范围</param>
      /// <returns>抖动时间</returns>
      /// <remarks>抖动时间可以用于避免缓存集体失效的情况</remarks>
      private TimeSpan Jitter( TimeSpan time )
      {
        var range = (int) Math.Min( time.TotalMilliseconds / 20, TimeSpan.FromHours( 1 ).TotalMilliseconds );
        var value = random.Next( range ) - range / 2;

        return TimeSpan.FromMilliseconds( value );
      }
    }



    /// <summary>
    /// 创建一个新的缓存策略，使用指定的优先级
    /// </summary>
    /// <param name="priority">默认缓存优先级</param>
    /// <returns>应用了默认缓存优先级的缓存策略提供程序</returns>
    public CachePolicy WithPriority( CachePriority priority )
    {
      return new CachePolicyProviderWithPriority( this, priority );
    }


    /// <summary>
    /// 创建一个新的缓存策略，使用高优先级
    /// </summary>
    /// <returns>使用高优先级的缓存策略</returns>
    public CachePolicy WithHighPriority()
    {
      return WithPriority( CachePriority.HighPriority );
    }


    private class CachePolicyProviderWithPriority : CachePolicy
    {
      private CachePriority _priority;
      private CachePolicy _provider;

      public CachePolicyProviderWithPriority( CachePolicy provider, CachePriority priority )
      {
        _provider = provider;
        _priority = priority;
      }

      public override CachePolicyItem CreatePolicyItem( string cacheKey, object cacheValue )
      {
        return _provider.CreatePolicyItem( cacheKey, cacheValue ).SetPriority( _priority );
      }
    }
  }

}
