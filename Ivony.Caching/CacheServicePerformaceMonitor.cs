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



    /// <summary>
    /// 总命中数
    /// </summary>
    public int TotalHits { get; private set; }

    /// <summary>
    /// 总未命中数
    /// </summary>
    public int TotalMisses { get; private set; }

    /// <summary>
    /// 命中率
    /// </summary>
    public double HitRate
    {
      get
      {
        //不直接使用属性计算是因为避免并发冲突
        var hits = (double) TotalHits;
        var total = hits + (double) TotalMisses;

        return hits / total;
      }
    }



    private object sync = new object();


    private void StatisticPerformace()
    {

      lock ( sync )
      {
        //性能计数统计活动必须同步进行
        foreach ( var entry in waitForStatisticsData )
        {
          if ( entry.hitted )
            TotalHits++;
          else
            TotalMisses++;
        }


        //先将当前容器放到 waitForStatisticsData 暂存，但此时仍可能有写入操作，所以到下一个周期再取出数据进行处理。
        waitForStatisticsData = realTimeData;
        realTimeData = new ConcurrentBag<Entry>();
      }
    }
  }
}
