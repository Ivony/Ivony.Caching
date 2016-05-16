using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  /// <summary>
  /// 定义一个异步的缓存服务
  /// </summary>
  public interface IAsyncCacheService
  {


    /// <summary>
    /// 获取缓存值
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="defaultValue">默认值（若获取不到则使用这个）</param>
    /// <param name="cancellationToken">用于取消异步操作的标识</param>
    /// <returns></returns>
    Task<T> Fetch<T>( string cacheKey, T defaultValue = default( T ), CancellationToken cancellationToken = default( CancellationToken ) );


    /// <summary>
    /// 获取或添加缓存值
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="valueFactory">创建缓存值的方法</param>
    /// <param name="cachePolicy">缓存策略</param>
    /// <param name="cancellationToken">用于取消异步操作的标识</param>
    /// <returns></returns>
    Task<T> FetchOrAdd<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy cachePolicy, CancellationToken cancellationToken = default( CancellationToken ) );

    /// <summary>
    /// 更新缓存值
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="valueFactory">创建缓存值的方法</param>
    /// <param name="cachePolicy">缓存策略</param>
    /// <param name="cancellationToken">用于取消异步操作的标识</param>
    /// <returns></returns>
    Task Update<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy cachePolicy, CancellationToken cancellationToken = default( CancellationToken ) );


    /// <summary>
    /// 清空缓存
    /// </summary>
    /// <returns></returns>
    Task Clear();


    /// <summary>
    /// 移除指定缓存键的缓存
    /// </summary>
    /// <param name="cacheKey">要移除的缓存项的键</param>
    /// <returns></returns>
    Task Remove( string cacheKey );
  }
}
