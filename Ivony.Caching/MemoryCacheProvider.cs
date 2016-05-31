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
    public MemoryCacheProvider( string name, Configuration configuration )
    {
      _name = name;
      _host = new MemoryCache( name, configuration == null ? null : configuration.ConfigurationData );
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
    public void Set( string key, object value, CachePolicyItem cachePolicy )
    {
      _host.Set( new System.Runtime.Caching.CacheItem( key, value ), CreateCacheItemPolicy( cachePolicy ) );
    }

    /// <summary>
    /// 根据现有缓存策略创建 MemoryCache 的缓存策略
    /// </summary>
    /// <param name="cachePolicy">缓存策略</param>
    /// <returns>MemoryCache 的缓存策略</returns>
    private CacheItemPolicy CreateCacheItemPolicy( CachePolicyItem cachePolicy )
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
    /// 获取 MemoryCache 对象
    /// </summary>
    public MemoryCache MemoryCache
    {
      get { return _host; }
    }



    /// <summary>
    /// MemoryCache 配置信息
    /// </summary>
    public sealed class Configuration
    {


      internal NameValueCollection ConfigurationData { get; private set; }


      /// <summary>
      /// 创建 Configuration 对象
      /// </summary>
      public Configuration()
      {
        ConfigurationData = new NameValueCollection();
      }


      /// <summary>
      /// 获取指定配置的值
      /// </summary>
      /// <param name="name">配置名</param>
      /// <returns>配置值</returns>
      public string this[string name]
      {
        get { return ConfigurationData[name]; }

        set { ConfigurationData[name] = value; }
      }


      /// <summary>
      /// 缓存内存限制 MB
      /// </summary>
      public int? CacheMemoryLimitMegabytes
      {
        get
        {
          int result;
          if ( int.TryParse( ConfigurationData["CacheMemoryLimitMegabytes"], out result ) )
            return result;

          else
            return null;
        }
        set
        {
          if ( value == null )
            ConfigurationData["CacheMemoryLimitMegabytes"] = null;
          else
            ConfigurationData["CacheMemoryLimitMegabytes"] = value.ToString();

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
          if ( int.TryParse( ConfigurationData["PhysicalMemoryLimitPercentage"], out result ) )
            return result;

          else
            return null;
        }
        set
        {
          if ( value < 0 || value > 100 )
            throw new ArgumentOutOfRangeException( "value" );

          if ( value == null )
            ConfigurationData["PhysicalMemoryLimitPercentage"] = null;
          else
            ConfigurationData["PhysicalMemoryLimitPercentage"] = value.ToString();

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
          if ( int.TryParse( ConfigurationData["PollingInterval"], out result ) )
            return result;

          else
            return null;
        }
        set
        {
          if ( value == null )
            ConfigurationData["PollingInterval"] = null;
          else
            ConfigurationData["PollingInterval"] = value.ToString();

        }
      }
    }
  }
}
