using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public class AsyncMultiCacheProvider : IAsyncCacheProvider
  {


    private IAsyncCacheProvider L1;
    private IAsyncL2CacheProvider L2;


    public Task Clear()
    {
      return Task.WhenAll( L1.Clear(), L2.Clear() );
    }

    public async Task<object> Get( string key )
    {
      var value = await L1.Get( key );

      if ( value != null )
        return value;

      var cacheItem = await L2.GetCacheItem( key );
      if ( cacheItem == null )
        return null;


      //后台写入一级缓存
      Background( L1.Set( cacheItem ) );
      return cacheItem.Value;
    }

    public Task Remove( string cacheKey )
    {

      return Task.WhenAll( L1.Remove( cacheKey ), L2.Remove( cacheKey ) );

    }

    public async Task Set( string key, object value, CachePolicy cachePolicy )
    {

      var cacheItem = new CacheItem( key, value, cachePolicy );

      //清除一级缓存，写入二级缓存

      await L1.Remove( key );
      await L2.Set( cacheItem );

      //后台写入一级缓存。
      Background( L1.Set( cacheItem ) );

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
