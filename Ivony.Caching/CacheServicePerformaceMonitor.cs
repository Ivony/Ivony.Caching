using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  /// <summary>
  /// CacheService 性能计数监视器
  /// </summary>
  public class CacheServicePerformaceMonitor : ICacheServiceMonitor
  {



    /// <summary>
    /// 创建 CacheServicePerformaceMonitor 实例 
    /// </summary>
    /// <param name="startImmediate">是否立即启动性能计数</param>
    public CacheServicePerformaceMonitor( bool startImmediate = true )
    {
      if ( startImmediate )
        Start();

      Task.Run( () => StatisticPerformaceLoop() );
    }


    /// <summary>
    /// 启动性能计数
    /// </summary>
    public void Start()
    {
      stopping = false;
    }


    /// <summary>
    /// 暂停性能计数
    /// </summary>
    public void Stop()
    {
      stopping = true;
    }



    private struct Entry
    {
      public string cacheKey;

      public bool hitted;
    }



    private ConcurrentBag<Entry> realTimeData = new ConcurrentBag<Entry>();
    private ConcurrentBag<Entry> waitForStatisticsData;


    private bool stopping = true;



    void ICacheServiceMonitor.OnCacheHitted( string cacheKey )
    {
      if ( stopping )
        return;

      realTimeData.Add( new Entry { cacheKey = cacheKey, hitted = true } );
    }

    void ICacheServiceMonitor.OnCacheMissed( string cacheKey )
    {
      if ( stopping )
        return;

      realTimeData.Add( new Entry { cacheKey = cacheKey, hitted = false } );
    }



    private async Task StatisticPerformaceLoop()
    {

      while ( true )
      {
        await Task.Delay( TimeSpan.FromSeconds( 1 ) );

        StatisticPerformace();
      }
    }



    private void StatisticPerformace()
    {

      lock ( sync )
      {

        if ( waitForStatisticsData != null )
        {

          //性能计数统计活动必须同步进行
          foreach ( var entry in waitForStatisticsData )
          {

            var cacheKey = entry.cacheKey;

            if ( entry.hitted )
            {
              globalHits++;
              if ( itemsHits.ContainsKey( entry.cacheKey ) == false )
                itemsHits.Add( cacheKey, 1 );
              else
                itemsHits[cacheKey]++;
            }
            else
            {
              globalMisses++;
              if ( itemsMisses.ContainsKey( entry.cacheKey ) == false )
                itemsMisses.Add( cacheKey, 1 );
              else
                itemsMisses[cacheKey]++;
            }
          }
        }

        //先将当前容器放到 waitForStatisticsData 暂存，但此时仍可能有写入操作，所以到下一个周期再取出数据进行处理。
        waitForStatisticsData = realTimeData;
        realTimeData = new ConcurrentBag<Entry>();
      }
    }




    private object sync = new object();
    private Dictionary<string, long> itemsHits = new Dictionary<string, long>();
    private Dictionary<string, long> itemsMisses = new Dictionary<string, long>();


    private long globalHits;
    private long globalMisses;


    /// <summary>
    /// 获取指定缓存键的命中数量
    /// </summary>
    /// <param name="cacheKey">缓存键，若不指定则获取全局数据</param>
    /// <returns>命中数量</returns>
    public long Hits( string cacheKey = null )
    {
      if ( cacheKey == null )
        return globalHits;

      else if ( itemsHits.ContainsKey( cacheKey ) )
        return itemsHits[cacheKey];

      else
        return 0;
    }


    /// <summary>
    /// 获取指定缓存键的未命中数量
    /// </summary>
    /// <param name="cacheKey">缓存键，若不指定则获取全局数据</param>
    /// <returns>未命中数量</returns>
    public long Misses( string cacheKey = null )
    {
      if ( cacheKey == null )
        return globalMisses;

      else if ( itemsMisses.ContainsKey( cacheKey ) )
        return itemsMisses[cacheKey];

      else
        return 0;
    }


    /// <summary>
    /// 获取指定缓存键的命中率
    /// </summary>
    /// <param name="cacheKey">缓存键，若不指定则获取全局数据</param>
    /// <returns>未命中数量</returns>
    public double HitRate( string cacheKey = null )
    {

      double hits, misses;

      if ( cacheKey == null )
      {
        hits = globalMisses;
        misses = globalMisses;
      }
      else
      {
        if ( itemsHits.ContainsKey( cacheKey ) )
          hits = itemsHits[cacheKey];
        else
          hits = 0;

        if ( itemsMisses.ContainsKey( cacheKey ) )
          misses = itemsMisses[cacheKey];
        else
          misses = 0;
      }


      if ( hits == 0 )//避免除零错误
        return 0;


      return hits / ( hits + misses );
    }
  }
}
