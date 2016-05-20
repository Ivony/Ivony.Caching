using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  /// <summary>
  /// 代表一个缓存项
  /// </summary>
  /// <typeparam name="T">缓存值类型</typeparam>
  public struct CacheEntry<T>
  {

    /// <summary>
    /// 创建一个缓存项
    /// </summary>
    /// <param name="value">缓存值</param>
    /// <param name="policy">缓存策略</param>
    public CacheEntry( T value, CachePolicyItem policy )
    {
      _value = value;
      _policy = policy;

      if ( _policy == null )
        throw new ArgumentNullException( "policy" );
    }



    private T _value;
    /// <summary>
    /// 缓存值
    /// </summary>
    public T Value { get { return _value; } }



    private CachePolicyItem _policy;
    /// <summary>
    /// 缓存策略项
    /// </summary>
    public CachePolicyItem CachePolicyItem { get { return _policy; } }


    /// <summary>
    /// 检查 CacheEntry 对象是否合法，若未被初始化或者被错误的初始化，则抛出异常
    /// </summary>
    public void Validate()
    {
      if ( _policy == null )
        throw new InvalidOperationException( "invalid cache entry." );
    }

  }
}
