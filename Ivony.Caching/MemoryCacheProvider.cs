using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// <param name="name">MemoryCache 的配置名称</param>
    /// <param name="configuration">MemoryCache 配置信息</param>
    public MemoryCacheProvider( string name, MemoryCacheConfiguration configuration )
    {
      _name = name;
      _host = new MemoryCache( name, configuration == null ? null : configuration.Configuration );
    }



    /// <summary>
    /// 创建内存缓存提供程序对象
    /// </summary>
    /// <param name="name">MemoryCache 的配置名称</param>
    /// <param name="configuration">MemoryCache 配置信息</param>
    public MemoryCacheProvider( string name, NameValueCollection configuration = null )
    {
      _name = name;
      _host = new MemoryCache( name, configuration );
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
      return new CacheItemPolicy
      {
        AbsoluteExpiration = cachePolicy.Expires,
        Priority = cachePolicy.Priority.IsHighPriority ? CacheItemPriority.NotRemovable : CacheItemPriority.Default,
      };
    }



    /// <summary>
    /// 回收所有非托管资源
    /// </summary>
    public void Dispose()
    {
      _host.Dispose();
    }



    /// <summary>
    /// MemoryCache 配置信息
    /// </summary>
    public sealed class MemoryCacheConfiguration
    {


      internal NameValueCollection Configuration { get; private set; }


      public MemoryCacheConfiguration()
      {
        Configuration = new NameValueCollection();
      }

      public string this[string key]
      {
        get { return Configuration[key]; }

        set { Configuration[key] = value; }
      }


      /// <summary>
      /// 缓存内存限制 MB
      /// </summary>
      public int? CacheMemoryLimitMegabytes
      {
        get
        {
          int result;
          if ( int.TryParse( Configuration["CacheMemoryLimitMegabytes"], out result ) )
            return result;

          else
            return null;
        }
        set
        {
          if ( value == null )
            Configuration["CacheMemoryLimitMegabytes"] = null;
          else
            Configuration["CacheMemoryLimitMegabytes"] = value.ToString();

        }
      }

      /// <summary>
      /// 物理内存限制百分比
      /// </summary>
      public int? PhysicalMemoryLimitPercentage
      {
        get
        {
          int result;
          if ( int.TryParse( Configuration["PhysicalMemoryLimitPercentage"], out result ) )
            return result;

          else
            return null;
        }
        set
        {
          if ( value < 0 || value > 100 )
            throw new ArgumentOutOfRangeException( "value" );

          if ( value == null )
            Configuration["PhysicalMemoryLimitPercentage"] = null;
          else
            Configuration["PhysicalMemoryLimitPercentage"] = value.ToString();

        }
      }


      /// <summary>
      /// 内存限制检查间隔时间
      /// </summary>
      public int? PollingInterval
      {
        get
        {
          int result;
          if ( int.TryParse( Configuration["PollingInterval"], out result ) )
            return result;

          else
            return null;
        }
        set
        {
          if ( value == null )
            Configuration["PollingInterval"] = null;
          else
            Configuration["PollingInterval"] = value.ToString();

        }
      }
    }
  }
}
