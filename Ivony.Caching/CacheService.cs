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
  /// 提供一个标准的基于异步的数据缓存的服务
  /// </summary>
  public class CacheService : IAsyncCacheService
  {


    private readonly Dictionary<string, Task> _tasks = new Dictionary<string, Task>();

    private readonly IAsyncCacheProvider _cacheProvider;

    private readonly object _sync = new object();




    private readonly ConcurrentBag<ICacheServiceMonitor> _monitors = new ConcurrentBag<ICacheServiceMonitor>();



    /// <summary>
    /// 注册一个 CacheServiceMonitor
    /// </summary>
    /// <param name="monitor">要注册的 CacheServiceMonitor 对象</param>
    /// <returns>返回自身便于链式调用</returns>
    public CacheService RegisterMonitor( ICacheServiceMonitor monitor )
    {
      _monitors.Add( monitor );
      return this;
    }





    /// <summary>
    /// 包装获取的值
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
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
    /// 创建异步缓存服务
    /// </summary>
    /// <param name="cacheProvider">异步缓存值提供程序</param>
    public CacheService( IAsyncCacheProvider cacheProvider, ICachePolicyProvider defaultPolicy = null )
    {
      _cacheProvider = cacheProvider;
      DefaultCachePolicyProvider = defaultPolicy ?? new NullCachePolicyProvider();
    }


    /// <summary>
    /// 创建异步缓存服务
    /// </summary>
    /// <param name="cacheProvider">缓存值提供程序</param>
    public CacheService( ICacheProvider cacheProvider, ICachePolicyProvider defaultPolicy = null )
    {
      _cacheProvider = cacheProvider.AsAsyncProvider();
      DefaultCachePolicyProvider = defaultPolicy ?? new NullCachePolicyProvider();
    }




    /// <summary>
    /// 默认缓存策略提供程序
    /// </summary>
    protected ICachePolicyProvider DefaultCachePolicyProvider { get; private set; }



    private sealed class NullCachePolicyProvider : ICachePolicyProvider
    {
      public CachePolicy CreateCachePolicy( string cacheKey, object cacheValue )
      {
        return null;
      }


    }

    private static Exception NoCachePolicy()
    {
      return new ArgumentException( "cachePolicy", "It's not set a default cache policy provider or not provided a valid policy, and also not specified by argument." );
    }


    /// <summary>
    /// 创建一个设置缓存值的任务
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="valueFactory">创建缓存值的工厂</param>
    /// <param name="cachePolicy">缓存策略</param>
    /// <returns>一个创建和设置缓存值的任务</returns>
    private async Task<T> SetValueAsync<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy cachePolicy = null )
    {
      await Task.Yield();
      var value = await valueFactory();

      cachePolicy = cachePolicy ?? DefaultCachePolicyProvider.CreateCachePolicy( cacheKey, value );
      if ( cachePolicy == null )
        throw NoCachePolicy();

      await _cacheProvider.Set( cacheKey, value, cachePolicy );

      return value;
    }




    private async Task<Result<T>> GetValueAsync<T>( string cacheKey )
    {
      var value = await _cacheProvider.Get( cacheKey );

      if ( value != null && value is T )
      {
        OnCacheHit( cacheKey );
        return new Result<T>( (T) value );
      }
      else
      {
        OnCacheMiss( cacheKey );
        return new Result<T>();
      }
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
    public async Task Set<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy policy = null, CancellationToken cancellationToken = default( CancellationToken ) )
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
    public async Task<T> FetchOrAdd<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy policy = null, CancellationToken cancellationToken = default( CancellationToken ) )
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


      try
      {
        await task;
      }
      finally
      {
        _tasks.Remove( cacheKey );
      }

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

      try
      {
        await task;
      }
      finally
      {
        _tasks.Remove( cacheKey );
      }
      return await Fetch( cacheKey, defaultValue, cancellationToken );
    }




    /// <summary>
    /// 清除指定缓存键的缓存项
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    public Task Remove( string cacheKey )
    {

      return _cacheProvider.Remove( cacheKey );

    }


    /// <summary>
    /// 清除所有的缓存项
    /// </summary>
    public Task Clear()
    {

      return _cacheProvider.Clear();

    }





    /// <summary>
    /// 当缓存命中时调用此方法
    /// </summary>
    /// <param name="cacheKey"></param>
    protected void OnCacheHit( string cacheKey )
    {
      //不理会 CahceServiceMonitor 执行过程中出现的异常
      foreach ( var item in _monitors )
        Task.Run( () => item.OnCacheHitted( cacheKey ) );
    }


    /// <summary>
    /// 当缓存未命中时调用此方法
    /// </summary>
    /// <param name="cacheKey"></param>
    protected void OnCacheMiss( string cacheKey )
    {
      //不理会 CahceServiceMonitor 执行过程中出现的异常
      foreach ( var item in _monitors )
        Task.Run( () => item.OnCacheMissed( cacheKey ) );
    }

  }
}
