using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{


  /// <summary>
  /// 实现一个使用内存缓存的缓存提供程序
  /// </summary>
  public class MemoryCacheProvider : ICacheProvider
  {

    private MemoryCache _host;

    private string _name;

    /// <summary>
    /// 创建内存缓存提供程序对象
    /// </summary>
    /// <param name="name"></param>
    public MemoryCacheProvider( string name )
    {
      _name = name;
      _host = new MemoryCache( name );
    }

    /// <summary>
    /// 清空缓存
    /// </summary>
    public void Clear()
    {
      _host = new MemoryCache( _name );
    }


    /// <summary>
    /// 获取缓存值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <returns></returns>
    public object Get( string key )
    {
      return _host.Get( key );
    }

    /// <summary>
    /// 移除指定键的缓存值
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    public void Remove( string cacheKey )
    {
      _host.Remove( cacheKey );
    }

    /// <summary>
    /// 设置缓存值
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="cachePolicy">缓存策略</param>
    public void Set( string key, object value, CachePolicy cachePolicy )
    {
      _host.Set( new System.Runtime.Caching.CacheItem( key, value ), CreateCacheItemPolicy( cachePolicy ) );
    }

    /// <summary>
    /// 根据现有缓存策略创建 MemoryCache 的缓存策略
    /// </summary>
    /// <param name="cachePolicy">缓存策略</param>
    /// <returns>MemoryCache 的缓存策略</returns>
    private CacheItemPolicy CreateCacheItemPolicy( CachePolicy cachePolicy )
    {
      return new CacheItemPolicy { AbsoluteExpiration = DateTime.UtcNow.AddHours( 1 ) };
    }


    public void Dispose()
    {
      _host.Dispose();
    }
  }
}
