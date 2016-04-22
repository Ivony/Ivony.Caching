using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching.Redis
{


  /// <summary>
  /// 定义一个文本序列化器
  /// </summary>
  public interface ITextSerializer
  {

    /// <summary>
    /// 将对象序列化成文本字符串
    /// </summary>
    /// <param name="obj">要序列化的对象</param>
    /// <returns>序列化文本</returns>
    string Serialize( object obj );


    /// <summary>
    /// 从序列化文本字符串中反序列化对象
    /// </summary>
    /// <param name="value">序列化文本</param>
    /// <returns>对象</returns>
    object Deserialize( string value );
  }
}
