using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{


  /// <summary>
  /// 实现异步多级缓存提供程序
  /// </summary>
  public class AsyncMultiCacheProvider : IAsyncCacheProvider
  {

    /// <summary>
    /// 一级缓存提供程序
    /// </summary>
    public IAsyncCacheProvider L1Cache { get; }



    /// <summary>
    /// 二级缓存提供程序
    /// </summary>
    public IAsyncL2CacheProvider L2Cache { get; }



    /// <summary>
    /// 创建多级缓存提供程序对象
    /// </summary>
    /// <param name="l1Cache">一级缓存</param>
    /// <param name="l2Cache">二级缓存</param>
    public AsyncMultiCacheProvider( IAsyncCacheProvider l1Cache, IAsyncL2CacheProvider l2Cache )
    {
      L1Cache = l1Cache;
      L2Cache = l2Cache;
    }


    public virtual Task Clear()
    {
      return Task.WhenAll( L1Cache.Clear(), L2Cache.Clear() );
    }

    public virtual async Task<object> Get( string key )
    {
      var value = await L1Cache.Get( key );

      if ( value != null )
        return value;

      var cacheItem = await L2Cache.GetCacheItem( key );
      if ( cacheItem == null )
        return null;


      //后台写入一级缓存
      Background( L1Cache.Set( cacheItem ) );
      return cacheItem.Value;
    }

    public virtual Task Remove( string cacheKey )
    {

      return Task.WhenAll( L1Cache.Remove( cacheKey ), L2Cache.Remove( cacheKey ) );

    }

    public virtual async Task Set( string key, object value, CachePolicy cachePolicy )
    {

      var cacheItem = new CacheItem( key, value, cachePolicy );

      //清除一级缓存，写入二级缓存

      await L1Cache.Remove( key );
      await L2Cache.Set( cacheItem );

      //后台写入一级缓存。
      Background( L1Cache.Set( cacheItem ) );

    }



    /// <summary>
    /// 在后台执行某任务
    /// </summary>
    /// <param name="task">要执行的任务</param>
    private async void Background( Task task )
    {
      await Task.Yield();
      await task;
    }

  }
}
