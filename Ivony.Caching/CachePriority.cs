using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
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
