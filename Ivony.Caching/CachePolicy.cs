using System;

namespace Ivony.Caching
{

  /// <summary>
  /// 定义缓存策略
  /// </summary>
  public class CachePolicy
  {

    /// <summary>
    /// 创建缓存策略
    /// </summary>
    /// <param name="expires">过期时间</param>
    protected CachePolicy( DateTime expires )
    {

      if ( expires.Kind != DateTimeKind.Utc )
        throw new ArgumentException( "expires must be an UTC time.", "expires" );


      Expires = expires;

    }


    /// <summary>
    /// 缓存失效时间
    /// </summary>
    public DateTime Expires { get; }



    public static CachePolicy Expired( DateTime expires )
    {
      return new CachePolicy( expires );
    }

    public static CachePolicy Expired( DateTimeOffset expires )
    {
      return new CachePolicy( expires.UtcDateTime );
    }


    public static CachePolicy Expired( TimeSpan expireTime )
    {
      if ( expireTime < TimeSpan.Zero )
        throw new ArgumentOutOfRangeException( "expireTime" );

      return new CachePolicy( DateTime.UtcNow + expireTime );
    }
  }
}