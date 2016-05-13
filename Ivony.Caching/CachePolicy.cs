using System;
using System.Runtime.Caching;

namespace Ivony.Caching
{

  /// <summary>
  /// 定义缓存策略
  /// </summary>
  public sealed class CachePolicy
  {

    /// <summary>
    /// 创建缓存策略
    /// </summary>
    /// <param name="expires">过期时间</param>
    /// <param name="priority">缓存优先级</param>
    private CachePolicy( DateTime expires, CachePriority priority = default( CachePriority ) )
    {

      if ( expires.Kind != DateTimeKind.Utc )
        throw new ArgumentException( "expires must be an UTC time.", "expires" );


      Expires = expires;
      Priority = priority;

    }


    /// <summary>
    /// 缓存失效时间
    /// </summary>
    public DateTime Expires { get; }


    /// <summary>
    /// 缓存目前是否有效
    /// </summary>
    public bool IsValid { get { return Expires > DateTime.UtcNow; } }


    /// <summary>
    /// 缓存优先级
    /// </summary>
    public CachePriority Priority { get; }



    /// <summary>
    /// 创建在指定时间后过期的缓存策略
    /// </summary>
    /// <param name="expires">过期时间</param>
    /// <returns>指定时间后过期的缓存策略</returns>
    public static CachePolicy Expired( DateTime expires )
    {
      return new CachePolicy( expires );
    }

    /// <summary>
    /// 创建在指定时间后过期的缓存策略
    /// </summary>
    /// <param name="expires">过期时间</param>
    /// <returns>指定时间后过期的缓存策略</returns>
    public static CachePolicy Expired( DateTimeOffset expires )
    {
      return new CachePolicy( expires.UtcDateTime );
    }


    /// <summary>
    /// 创建在指定有效期的缓存策略
    /// </summary>
    /// <param name="expireTime">有效期</param>
    /// <returns>只有指定有效期的缓存策略</returns>
    public static CachePolicy Expired( TimeSpan expireTime )
    {
      if ( expireTime < TimeSpan.Zero )
        throw new ArgumentOutOfRangeException( "expireTime" );

      return new CachePolicy( DateTime.UtcNow + expireTime );
    }







    /// <summary>
    /// 将缓存优先级设置为高优先级
    /// </summary>
    /// <returns>设置为高优先级之后的缓存策略</returns>
    public CachePolicy HighPriority()
    {
      return new CachePolicy( Expires, CachePriority.HighPriority );
    }


    /// <summary>
    /// 将缓存优先级设置为低优先级
    /// </summary>
    /// <returns>设置为低优先级之后的缓存策略</returns>
    public CachePolicy LowPriority()
    {
      return new CachePolicy( Expires, CachePriority.LowPriority );
    }


    /// <summary>
    /// 将缓存优先级设置为自定义优先级
    /// </summary>
    /// <param name="priority">自定义优先级</param>
    /// <returns>设置为自定义优先级之后的缓存策略</returns>
    public CachePolicy SetPriority( CachePriority priority )
    {
      return new CachePolicy( Expires, priority );
    }




    private static readonly CachePolicy _invalid = CachePolicy.Expired( DateTime.MinValue.ToUniversalTime() );


    /// <summary>
    /// 获取一个已经失效的缓存策略
    /// </summary>
    public static CachePolicy Invalid { get { return _invalid; } }



    /// <summary>
    /// 从字符串中解析出缓存策略对象
    /// </summary>
    /// <param name="str">要解析的字符串</param>
    /// <returns>缓存策略对象</returns>
    public static CachePolicy Parse( string str )
    {
      if ( str == null )
        throw new ArgumentNullException( str );

      var data = str.Split( ',' );
      if ( data.Length != 2 )
        throw new FormatException();

      var expires = long.Parse( data[0] );
      var prioroty = CachePriority.Parse( data[1] );


      return new CachePolicy( new DateTime( expires, DateTimeKind.Utc ), prioroty );
    }


    /// <summary>
    /// 重写 ToString 方法，输出缓存策略字符串表达形式
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return string.Format( "{0},{1}", Expires.Ticks, Priority );
    }



  }

  /// <summary>
  /// 缓存优先级
  /// </summary>
  public struct CachePriority
  {


    private int _priorityValue;

    private string _hint;


    private CachePriority( int priorityValue, string hint = null )
    {
      _priorityValue = priorityValue;
      if ( hint != null && hint.Contains( "\n" ) )
        throw new ArgumentException( "hint cannot contains new-line character", "hint" );
      _hint = hint;
    }


    private static CachePriority _high = new CachePriority( 1 );
    /// <summary>
    /// 预置高优先级
    /// </summary>
    public static CachePriority HighPriority { get { return _high; } }


    /// <summary>
    /// 预置默认优先级
    /// </summary>
    public static CachePriority DefaultPriority { get { return new CachePriority(); } }


    private static CachePriority _low = new CachePriority( -1 );
    /// <summary>
    /// 预置低优先级
    /// </summary>
    public static CachePriority LowPriority { get { return _low; } }


    /// <summary>
    /// 获取一个值标识该缓存优先级是否为高优先级
    /// </summary>
    public bool IsHighPriority { get { return _priorityValue > 0; } }


    /// <summary>
    /// 获取一个值标识该缓存优先级是否为低优先级
    /// </summary>
    public bool IsLowPriority { get { return _priorityValue < 0; } }


    /// <summary>
    /// 创建一个自定义的缓存优先级
    /// </summary>
    /// <param name="priority">预制优先级，当缓存提供程序无法识别缓存优先级提示时使用</param>
    /// <param name="hint">缓存优先级提示信息，特定的缓存提供程序可以读取该信息使用最合适的优先级</param>
    /// <returns></returns>
    public CachePriority CreatePriority( CachePriority priority, string hint )
    {
      if ( priority._hint != null )
        throw new ArgumentException( "priority" );

      return new CachePriority( priority._priorityValue, hint );
    }


    /// <summary>
    /// 重写 Equals 方法，比较两个缓存优先级对象（注意，只要预置的优先级一样，就会判定两个对象相同，将忽略缓存优先级提示信息）
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>是否具有同等的优先级</returns>
    public override bool Equals( object obj )
    {
      if ( obj != null && obj is CachePriority )
        return Equals( (CachePriority) obj );

      else
        return false;
    }


    /// <summary>
    /// 重写 Equals 方法，比较两个缓存优先级对象（注意，只要预置的优先级一样，就会判定两个对象相同，将忽略缓存优先级提示信息）
    /// </summary>
    /// <param name="priority">要比较的对象</param>
    /// <returns>是否具有同等的优先级</returns>
    public bool Equals( CachePriority priority )
    {
      return priority._priorityValue == this._priorityValue;
    }


    /// <summary>
    /// 重写 GetHashCode 方法，获取正确的哈希值
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      return _priorityValue;
    }


    /// <summary>
    /// 重写 ToString 方法，输出优先级和优先级提示信息
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {

      string result = null;

      if ( _priorityValue == -1 )
        result = "Low";

      else if ( _priorityValue == 0 )
        result = "Default";

      else if ( _priorityValue == 1 )
        result = "High";


      if ( _hint != null )
        result += ":" + _hint;

      return result;
    }


    /// <summary>
    /// 从字符串中解析出缓存优先级信息
    /// </summary>
    /// <param name="str">要解析的字符串</param>
    /// <returns></returns>
    public static CachePriority Parse( string str )
    {

      var index = str.IndexOf( ':' );

      string value, hint;
      if ( index == -1 )
      {
        value = str;
        hint = null;
      }
      else
      {
        value = str.Remove( index );
        hint = str.Substring( index + 1 );
      }


      switch ( value )
      {
        case "Low":
          return new CachePriority( -1, hint );

        case "Default":
          return new CachePriority( 0, hint );

        case "High":
          return new CachePriority( 1, hint );

        default:
          throw new FormatException();
      }

    }
  }

}