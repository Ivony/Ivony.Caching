using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{


  /// <summary>
  /// 定义缓存无效策略
  /// </summary>
  public abstract class CacheDependency
  {

    /// <summary>
    /// 获取缓存项有效状态
    /// </summary>
    /// 
    public abstract CacheState GetCacheState();



    /// <summary>
    /// 从文本中解析出缓存无效策略
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static CacheDependency Parse( string text )
    {
      var items = text.Split( new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries ).Select( i => ParseItem( i ) ).ToArray();
      if ( items.Length == 1 )
        return items[0];

      else
        return new CombinedCacheInvalidationPolicy( items );
    }

    private static CacheDependency ParseItem( string text )
    {
      return null;
    }








    /// <summary>
    /// 合并多个缓存无效策略
    /// </summary>
    /// <param name="items">要合并的缓存无效策略</param>
    /// <returns>合并后的缓存无效策略</returns>
    public static CacheDependency Combine( params CacheDependency[] items )
    {
      if ( items == null )
        throw new ArgumentNullException( "items" );

      return new CombinedCacheInvalidationPolicy( items );
    }



    /// <summary>
    /// 合并两个缓存无效策略
    /// </summary>
    /// <param name="a">要合并的第一个项</param>
    /// <param name="b">要合并的第二个项</param>
    /// <returns>合并后的缓存无效策略</returns>
    public static CacheDependency operator +( CacheDependency a, CacheDependency b )
    {
      return Combine( a, b );
    }



    private class CombinedCacheInvalidationPolicy : CacheDependency
    {
      private CacheDependency[] _items;

      public CombinedCacheInvalidationPolicy( CacheDependency[] items )
      {

        _items = items.Where( i => i != null ).SelectMany( i =>
        {
          var combined = i as CombinedCacheInvalidationPolicy;
          if ( combined != null )
            return combined._items;

          else
            return new[] { i };
        } ).ToArray();

      }




      private CacheState _state = CacheState.Valid;

      public override CacheState GetCacheState()
      {

        if ( _state == CacheState.Invalid )
          return CacheState.Invalid;

        foreach ( var item in _items )
        {
          _state = CompareState( _state, item.GetCacheState() );
          if ( _state == CacheState.Invalid )
            return CacheState.Invalid;
        }


        return _state;
      }

      private CacheState CompareState( CacheState a, CacheState b )
      {
        if ( a == CacheState.Invalid || b == CacheState.Invalid )               //任何一个状态无效则无效
          return CacheState.Invalid;

        else if ( a == CacheState.NearInvalid || b == CacheState.NearInvalid )  //在没有无效状态的前提下，有接近无效状态则接近无效
          return CacheState.NearInvalid;

        else if ( a == CacheState.Valid && b == CacheState.Valid )              //只有两个状态都有效的时候才是有效的
          return CacheState.Valid;

        else                                                                                        //无法识别的状态都是无效
          return CacheState.Invalid;


      }
    }







    /// <summary>
    /// 从固定过期时间创建缓存无效策略
    /// </summary>
    /// <param name="expires">过期时间</param>
    /// <returns>缓存无效策略</returns>
    public static CacheDependency Create( DateTime expires )
    {
      if ( expires.Kind != DateTimeKind.Utc )
        throw new ArgumentException( "must be a utc datetime", "expires" );

      return new ExpiresPolicy( expires.ToUniversalTime() );
    }



    private class ExpiresPolicy : CacheDependency
    {

      private DateTime _expires;


      public ExpiresPolicy( DateTime expires )
      {

        if ( expires.Kind != DateTimeKind.Utc )
          throw new ArgumentException( "must be a utc datetime", "expires" );

        _expires = expires;
      }


      public DateTime Expires { get { return _expires; } }

      public override CacheState GetCacheState()
      {
        return DateTime.UtcNow > _expires ? CacheState.Valid : CacheState.Invalid;
      }
    }

  }


  /// <summary>
  /// 定义缓存项有效状态
  /// </summary>
  public enum CacheState
  {
    /// <summary>缓存项当前有效</summary>
    Valid,

    /// <summary>缓存项当前已经失效</summary>
    Invalid,

    /// <summary>缓存项即将失效，但目前仍然可用</summary>
    NearInvalid,

  }
}
