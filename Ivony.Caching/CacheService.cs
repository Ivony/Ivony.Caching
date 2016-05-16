using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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


    private readonly IAsyncCacheProvider _cacheProvider;

    private readonly object _sync = new object();

    private readonly TaskManager _tasks = new TaskManager();




    private readonly ConcurrentBag<ICacheMonitor> _monitors = new ConcurrentBag<ICacheMonitor>();



    /// <summary>
    /// 注册一个 CacheServiceMonitor
    /// </summary>
    /// <param name="monitor">要注册的 CacheServiceMonitor 对象</param>
    /// <returns>返回自身便于链式调用</returns>
    public CacheService RegisterMonitor( ICacheMonitor monitor )
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
    /// <param name="cacheProvider">异步缓存提供程序</param>
    /// <param name="defaultPolicy">默认缓存策略</param>
    public CacheService( IAsyncCacheProvider cacheProvider, CachePolicy defaultPolicy = null )
    {
      _cacheProvider = cacheProvider;
      DefaultCachePolicy = defaultPolicy ?? new NullCachePolicyProvider();
    }


    /// <summary>
    /// 创建异步缓存服务
    /// </summary>
    /// <param name="cacheProvider">同步缓存提供程序</param>
    /// <param name="defaultPolicy">默认缓存策略</param>
    public CacheService( ICacheProvider cacheProvider, CachePolicy defaultPolicy = null )
    {
      _cacheProvider = cacheProvider.AsAsyncProvider();
      DefaultCachePolicy = defaultPolicy ?? new NullCachePolicyProvider();
    }




    /// <summary>
    /// 默认缓存策略提供程序
    /// </summary>
    public CachePolicy DefaultCachePolicy { get; private set; }



    private sealed class NullCachePolicyProvider : CachePolicy
    {
      public override CachePolicyItem CreatePolicyItem( string cacheKey, object cacheValue )
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
    private async Task<T> SetValue<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy cachePolicy )
    {
      await Task.Yield();

      var value = await valueFactory();

      var policy = (cachePolicy ?? DefaultCachePolicy).CreatePolicyItem( cacheKey, value );
      if ( policy == null )
        throw NoCachePolicy();


      if ( policy.CacheState != CacheState.Invalid )
        await _cacheProvider.Set( cacheKey, value, policy );


      return value;
    }




    private async Task<Result<T>> GetValue<T>( string cacheKey )
    {
      await Task.Yield();

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
    public async Task Update<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy policy = null, CancellationToken cancellationToken = default( CancellationToken ) )
    {

      bool running = true;
      await _tasks.GetOrAdd( cacheKey, () =>
      {
        running = false;
        return SetValue( cacheKey, valueFactory, policy );
      } );


      if ( running )
        await Update( cacheKey, valueFactory, policy, cancellationToken );

    }


    /// <summary>
    /// 获取或添加缓存项
    /// </summary>
    /// <typeparam name="T">缓存值类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="valueFactory">创建缓存值的工厂</param>
    /// <param name="policy">缓存策略</param>
    /// <param name="cancellationToken">取消标识</param>
    /// <returns></returns>
    public async Task<T> FetchOrAdd<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy policy = null, CancellationToken cancellationToken = default( CancellationToken ) )
    {

      var value = await GetValue<T>( cacheKey );
      if ( value.Success )
        return value.Value;



      var task = _tasks.GetOrAdd( cacheKey, () => SetValue( cacheKey, valueFactory, policy ) );
      await task;


      var resultTask = task as Task<T>;
      if ( resultTask != null )
        return resultTask.Result;

      else
        throw new InvalidCastException( string.Format( "cannot cast the setted value to type \"{0}\"", typeof( T ).AssemblyQualifiedName ) );
    }




    /// <summary>
    /// 尝试从缓存中获取一个值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="defaultValue">默认值（若获取不到值时则返回默认值）</param>
    /// <param name="cancellationToken">取消标识</param>
    /// <returns></returns>
    public async Task<T> Fetch<T>( string cacheKey, T defaultValue = default( T ), CancellationToken cancellationToken = default( CancellationToken ) )
    {


      var value = await GetValue<T>( cacheKey );
      if ( value.Success )
        return value.Value;



      var task = _tasks.GetOrAdd( cacheKey, () => Task.FromResult( defaultValue ) );
      await task;

      var resultTask = task as Task<T>;
      if ( resultTask != null )
        return resultTask.Result;

      else
        throw new InvalidCastException( string.Format( "cannot convert the setted value to type \"{0}\"", typeof( T ).AssemblyQualifiedName ) );
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
      //不理会 CahceMonitor 执行过程中出现的异常
      foreach ( var item in _monitors )
        Task.Run( () => item.OnCacheHitted( cacheKey ) );
    }


    /// <summary>
    /// 当缓存未命中时调用此方法
    /// </summary>
    /// <param name="cacheKey"></param>
    protected void OnCacheMiss( string cacheKey )
    {
      //不理会 CahceMonitor 执行过程中出现的异常
      foreach ( var item in _monitors )
        Task.Run( () => item.OnCacheMissed( cacheKey ) );
    }

  }
}
