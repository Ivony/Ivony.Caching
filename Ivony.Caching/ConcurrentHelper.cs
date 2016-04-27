using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  internal static class ConcurrentHelper
  {

    /// <summary>
    /// 从 ConcurrentDictionary 中获取或者新增一个值，且值必须满足指定断言，否则将不满足断言的值替换
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <param name="collection">一个线程安全的字典类型</param>
    /// <param name="key">用于索引值的键</param>
    /// <param name="value">要添加的值</param>
    /// <param name="assertion">值必须满足的断言</param>
    /// <returns></returns>
    internal static TValue GetOrAdd<TKey, TValue>( this ConcurrentDictionary<TKey, TValue> collection, TKey key, TValue value, Func<TValue, bool> assertion ) where TValue : class
    {
      if ( assertion( value ) == false )
        throw new ArgumentException( "value fails the assertion", "value" );


      while ( true )
      {
        var result = collection.GetOrAdd( key, value );
        if ( assertion( result ) )
          return result;

        collection.TryUpdate( key, value, result );
      }
    }


  }
}
