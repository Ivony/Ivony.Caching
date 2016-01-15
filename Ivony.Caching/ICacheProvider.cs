using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public interface ICacheProvider
  {
    /// <summary>
    /// 获取缓存值
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <returns>缓存值</returns>
    object Get( string cacheKey );

    /// <summary>
    /// 设置缓存值
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="cachePolicy">缓存策略</param>
    void Set( string cacheKey, object value, CachePolicy cachePolicy );

    /// <summary>
    /// 移除某个缓存值
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <returns>用于等待操作完成的 Task 对象</returns>
    void Remove( string cacheKey );

    /// <summary>
    /// 清空缓存
    /// </summary>
    /// <returns>用于等待操作完成的 Task 对象</returns>
    void Clear();

  }
}
