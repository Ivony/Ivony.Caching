using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  /// <summary>
  /// 定义一个 CacheService 监视器
  /// </summary>
  public interface ICacheServiceMonitor
  {

    /// <summary>
    /// 当缓存命中时调用此方法
    /// </summary>
    /// <param name="cacheKey">命中的缓存键</param>
    void OnCacheHitted( string cacheKey );

    /// <summary>
    /// 当缓存未命中时调用此方法
    /// </summary>
    /// <param name="cacheKey">未命中的缓存键</param>
    void OnCacheMissed( string cacheKey );

  }
}
