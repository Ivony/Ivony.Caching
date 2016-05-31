using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  /// <summary>
  /// 定义一个伪缓存提供程序，伪缓存提供程序不进行任何真正的缓存，对于所有的缓存值直接抛弃，并且永远返回 null
  /// </summary>
  public class SpuriousCacheProvider : ICacheProvider
  {
    void ICacheProvider.Clear()
    {
    }

    void IDisposable.Dispose()
    {
    }

    object ICacheProvider.Get( string cacheKey )
    {
      return null;
    }

    void ICacheProvider.Remove( string cacheKey )
    {
    }

    void ICacheProvider.Set( string cacheKey, object value, CachePolicyItem cachePolicy )
    {
    }



    private static SpuriousCacheProvider _instance = new SpuriousCacheProvider();

    /// <summary>
    /// 获取伪缓存提供程序的实例
    /// </summary>
    public static SpuriousCacheProvider Instance { get { return _instance; } }


    private SpuriousCacheProvider() { }

  }
}
