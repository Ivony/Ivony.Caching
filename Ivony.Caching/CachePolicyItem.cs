using System;
using System.Runtime.Caching;

namespace Ivony.Caching
{

  /// <summary>
  /// 定义缓存策略项
  /// </summary>
  public sealed class CachePolicyItem
  {

    /// <summary>
    /// 创建缓存策略
    /// </summary>
    /// <param name="expires">过期时间</param>
    /// <param name="dependencies">缓存依赖项</param>
    /// <param name="priority">缓存优先级</param>
    public CachePolicyItem( DateTime expires, CacheDependency dependencies = null, CachePriority priority = default( CachePriority ) )
    {
      if ( expires.Kind != DateTimeKind.Utc )
        throw new ArgumentException( "expires must be an UTC time.", "expires" );


      Expires = expires;
      Priority = priority;
      Dependencies = dependencies;
    }


    /// <summary>
    /// 缓存失效时间
    /// </summary>
    public DateTime Expires { get; }


    /// <summary>
    /// 缓存目前是否有效
    /// </summary>
    public bool IsValid { get { return Expires > DateTime.UtcNow && Dependencies.GetCacheState() == CacheState.Valid; } }


    /// <summary>
    /// 缓存依赖项
    /// </summary>
    public CacheDependency Dependencies { get; private set; }



    /// <summary>
    /// 缓存优先级
    /// </summary>
    public CachePriority Priority { get; }





    /// <summary>
    /// 将缓存优先级设置为自定义优先级
    /// </summary>
    /// <param name="priority">自定义优先级</param>
    /// <returns>设置为自定义优先级之后的缓存策略</returns>
    public CachePolicyItem SetPriority( CachePriority priority )
    {
      return new CachePolicyItem( Expires, Dependencies, priority );
    }




    private static readonly CachePolicyItem _invalidItem = new CachePolicyItem( DateTime.MinValue.ToUniversalTime() );


    /// <summary>
    /// 获取一个已经失效的缓存策略
    /// </summary>
    public static CachePolicyItem InvalidCachePolicy { get { return _invalidItem; } }



    /// <summary>
    /// 从字符串中解析出缓存策略对象
    /// </summary>
    /// <param name="str">要解析的字符串</param>
    /// <returns>缓存策略对象</returns>
    public static CachePolicyItem Parse( string str )
    {
      if ( str == null )
        throw new ArgumentNullException( str );

      var data = str.Split( ',' );
      if ( data.Length != 2 )
        throw new FormatException();

      var expires = long.Parse( data[0] );
      var prioroty = CachePriority.Parse( data[1] );


      return new CachePolicyItem( new DateTime( expires, DateTimeKind.Utc ), null, prioroty );
    }


    /// <summary>
    /// 获取缓存项当前状态
    /// </summary>
    /// <returns></returns>
    public CacheState CacheState
    {
      get
      {
        if ( DateTime.UtcNow > Expires )
          return CacheState.Invalid;

        if ( Dependencies == null )
          return CacheState.Valid;

        else
          return Dependencies.GetCacheState();
      }
    }


    /// <summary>
    /// 重写 ToString 方法，输出缓存策略字符串表达形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format( "{0},{1}", Expires.Ticks, Priority );
    }





    /// <summary>
    /// 允许将 CachePolicyItem 隐式转换为 CachePolicy
    /// </summary>
    /// <param name="policy">要转换的 CachePolicyItem 对象</param>
    public static implicit operator CachePolicy( CachePolicyItem policy )
    {

      if ( policy == null )
        return null;

      return new SpecifyCachePolicy( policy );
    }

    private class SpecifyCachePolicy : CachePolicy
    {

      private CachePolicyItem _item;

      public SpecifyCachePolicy( CachePolicyItem item )
      {
        _item = item;
      }

      public override CachePolicyItem CreatePolicyItem( string cacheKey, object cacheValue )
      {
        return _item;
      }
    }



  }


}