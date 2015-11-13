using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  /// <summary>
  /// 提供同步或异步的数据缓存的服务
  /// </summary>
  public class CacheService : IAsyncCacheService
  {


    private readonly Dictionary<string, Task> _tasks = new Dictionary<string, Task>();

    private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

    private readonly IAsyncCacheProvider _valueProvider;


    private readonly object _sync = new object();




    /// <summary>
    /// 包装获取的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    private struct Result<T>
    {
      public Result( T value )
      {
        Success = true;
        Value = value;
      }

      public bool Success { get; private set; }


      public T Value { get; private set; }

    }




    /// <summary>
    /// 创建一个设置缓存值的任务
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="valueFactory">创建缓存值的工厂</param>
    /// <returns>一个创建和设置缓存值的任务</returns>
    private async Task<T> SetValueAsync<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy cachePolicy )
    {
      await Task.Yield();
      var value = await valueFactory();
      await _valueProvider.Set( cacheKey, value, cachePolicy );

      return value;
    }




    private async Task<Result<T>> GetValueAsync<T>( string cacheKey )
    {
      var value = await _valueProvider.Get( cacheKey );

      if ( value != null && value is T )
        return new Result<T>( (T) value );

      else
        return new Result<T>();
    }





    /// <summary>
    /// 尝试设置一个缓存项
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="cacheKey"></param>
    /// <param name="valueFactory"></param>
    /// <param name="policy"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Set<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy policy, CancellationToken cancellationToken = default( CancellationToken ) )
    {
      Task task;
      if ( _tasks.TryGetValue( cacheKey, out task ) )
        await task;


      _tasks.Add( cacheKey, task = SetValueAsync( cacheKey, valueFactory, policy ) );
      await task;
    }


    /// <summary>
    /// 获取或添加缓存项
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="valueFactory">创建缓存值的工厂</param>
    /// <param name="cancellationToken">取消标识</param>
    /// <returns></returns>
    public async Task<T> FetchOrAdd<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy policy, CancellationToken cancellationToken = default( CancellationToken ) )
    {

      var value = await GetValueAsync<T>( cacheKey );
      if ( value.Success )
        return value.Value;


      Task task;
      lock ( _sync )
      {


        if ( _tasks.TryGetValue( cacheKey, out task ) == false )
        {
          _tasks.Add( cacheKey, task = SetValueAsync( cacheKey, valueFactory, policy ) );
        }
      }


      await task;

      var resultTask = task as Task<T>;
      if ( resultTask != null )
        return resultTask.Result;

      else
        return await FetchOrAdd( cacheKey, valueFactory, policy, cancellationToken );
    }



    public async Task<T> Fetch<T>( string cacheKey, T defaultValue = default( T ), CancellationToken cancellationToken = default( CancellationToken ) )
    {


      var value = await GetValueAsync<T>( cacheKey );
      if ( value.Success )
        return value.Value;




      Task task;
      lock ( _sync )
      {
        if ( _tasks.TryGetValue( cacheKey, out task ) == false )//当前没有设置值的话直接返回默认值
          return defaultValue;
      }

      await task;                                               //否则等待值设置完毕后再检查
      return await Fetch( cacheKey, defaultValue, cancellationToken );
    }




    /// <summary>
    /// 清除指定缓存键的缓存项
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    public Task ClearCache( string cacheKey )
    {

      return _valueProvider.Remove( cacheKey );

    }


    /// <summary>
    /// 清除所有的缓存项
    /// </summary>
    public Task ClearCache()
    {

      return _valueProvider.Clear();

    }
  }
}
