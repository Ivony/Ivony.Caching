using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  /// <summary>
  /// 基于磁盘文件的缓存提供程序
  /// </summary>
  public class DiskCacheProvider : IAsyncCacheProvider
  {

    private DiskCacheManager _manager;
    private IFormatter _serializer;


    /// <summary>
    /// 创建 DiskCacheProvider 对象
    /// </summary>
    /// <param name="path">磁盘缓存路径</param>
    public DiskCacheProvider( string path ) : this( path, new BinaryFormatter() ) { }


    /// <summary>
    /// 创建 DiskCacheProvider 对象
    /// </summary>
    /// <param name="path">磁盘缓存路径</param>
    /// <param name="serializer">二进制序列化器</param>
    public DiskCacheProvider( string path, IFormatter serializer )
    {
      if ( Path.IsPathRooted( path ) == false )
        throw new ArgumentException( "must be a rooted path", path );

      _manager = new DiskCacheManager( path );
      _serializer = serializer;

      _manager.AssignCacheDirectory();
    }





    /// <summary>
    /// 清除所有缓存
    /// </summary>
    /// <returns></returns>
    public Task Clear()
    {
      _manager.AssignCacheDirectory();//重新分配一个缓存路径即可
      return Task.FromResult( (object) null );
    }

    /// <summary>
    /// 释放所有资源，停止提供缓存
    /// </summary>
    public void Dispose()
    {
      _manager.Dispose();
      _manager = null;
    }

    /// <summary>
    /// 获取一个缓存值
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <returns>缓存值</returns>
    public async Task<object> Get( string cacheKey )
    {
      var error = _manager.ValidateCacheKey( cacheKey );
      if ( error != null )
        throw new ArgumentException( error, "cacheKey" );



      var cachePolicy = _manager.GetCachePolicy( cacheKey );
      if ( cachePolicy == null || cachePolicy.IsValid == false )
        return null;

      return _serializer.Deserialize( await _manager.ReadStream( cacheKey ) );
    }



    /// <summary>
    /// 移除一个缓存项
    /// </summary>
    /// <param name="cacheKey">要移除的缓存键</param>
    public Task Remove( string cacheKey )
    {
      _manager.Remove( cacheKey );
      return Task.FromResult( (object) null );
    }

    /// <summary>
    /// 设置一个缓存
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="cachePolicy">缓存策略</param>
    public async Task Set( string cacheKey, object value, CachePolicy cachePolicy )
    {

      _manager.SetCachePolicy( cacheKey, cachePolicy );

      var stream = new MemoryStream();
      _serializer.Serialize( stream, value );
      await _manager.WriteStream( cacheKey, stream );

    }
  }
}
