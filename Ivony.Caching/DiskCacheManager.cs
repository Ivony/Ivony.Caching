using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  /// <summary>
  /// 磁盘缓存管理器，磁盘缓存管理器负责打开调度文件打开和关闭，过期缓存清理等
  /// </summary>
  internal sealed class DiskCacheManager : IDisposable
  {


    /// <summary>
    /// 创建磁盘缓存管理器对象
    /// </summary>
    /// <param name="rootPath"></param>
    public DiskCacheManager( string rootPath )
    {
      RootPath = rootPath;
    }

    /// <summary>
    /// 分配一个新的缓存目录（一般用于清除缓存）
    /// </summary>
    internal void AssignCacheDirectory()
    {
      CurrentDirectory = Path.Combine( RootPath, Path.GetRandomFileName() );
      Directory.CreateDirectory( CurrentDirectory );
    }



    /// <summary>
    /// 缓存根目录
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// 当前目录
    /// </summary>
    public string CurrentDirectory { get; private set; }


    /// <summary>
    /// 读写缓冲区大小
    /// </summary>
    public int BufferSize { get; private set; } = 1024;



    private object _sync = new object();

    private ConcurrentDictionary<string, Task> actionTasks = new ConcurrentDictionary<string, Task>();


    /// <summary>
    /// 读取一个流
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <returns></returns>
    public async Task<Stream> ReadStream( string cacheKey )
    {
      var filepath = Path.Combine( CurrentDirectory, cacheKey );
      if ( File.Exists( filepath ) == false )
        return null;


      var cancellation = new CancellationTokenSource();
      var newTask = Task.Run( async () =>
      {
        await Task.Yield();
        return await ReadStream( File.OpenRead( filepath ) );
      }, cancellation.Token );


      var task = actionTasks.AddOrUpdate( cacheKey, newTask, ( key, item ) => item ?? newTask );
      if ( newTask != task )   //如果任务未能加入队列，则直接取消
        cancellation.Cancel();

      try
      {
        await task;
      }
      finally
      {
        actionTasks.TryUpdate( cacheKey, null, task );
      }


      var readTask = task as Task<Stream>;
      if ( readTask != null )                //如果当前正在读，则以当前读取结果返回。
        return readTask.Result;

      else                                   //如果当前正在写，等写完后再读取一次。
        return await ReadStream( cacheKey );

    }

    private async Task<Stream> ReadStream( FileStream stream )
    {
      using ( stream )
      {
        var result = new MemoryStream();

        var buffer = new byte[BufferSize];

        while ( true )
        {
          var size = await stream.ReadAsync( buffer, 0, buffer.Length );
          result.Write( buffer, 0, size );


          if ( size < buffer.Length )
            break;
        }

        result.Seek( 0, SeekOrigin.Begin );
        return result;
      }
    }

    public Task WriteStream( string cacheKey, byte[] data )
    {
      return WriteStream( cacheKey, new MemoryStream( data ) );
    }


    public async Task WriteStream( string cacheKey, MemoryStream data )
    {
      var filepath = Path.Combine( CurrentDirectory, cacheKey );
      Directory.CreateDirectory( CurrentDirectory );




      var cancellation = new CancellationTokenSource();
      var newTask = Task.Run( async () =>
      {
        await Task.Yield();
        await WriteStream( File.OpenWrite( filepath ), data );
      }, cancellation.Token );


      var task = actionTasks.AddOrUpdate( cacheKey, newTask, ( key, item ) => item ?? newTask );

      try
      {
        await task;
      }
      finally
      {
        actionTasks.TryUpdate( cacheKey, null, task );
      }


      if ( newTask != task )     //如果任务未能加入队列，则直接取消然后再尝试一次
      {
        cancellation.Cancel();
        await WriteStream( cacheKey, data );
      }

    }


    /// <summary>
    /// 将数据写入文件流
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="data">数据</param>
    /// <returns></returns>
    private async Task WriteStream( FileStream stream, MemoryStream data )
    {
      data.Seek( 0, SeekOrigin.Begin );
      using ( stream )
      {
        await data.CopyToAsync( stream, BufferSize );
      }
    }

    public string ValidateCacheKey( string cacheKey )
    {
      if ( cacheKey.IndexOfAny( Path.GetInvalidFileNameChars() ) >= 0 || cacheKey.Contains( '.' ) )
        return "cacheKey contains an invalid character";

      else
        return null;
    }

    public void Remove( string cacheKey )
    {
      var filepath = Path.Combine( CurrentDirectory, cacheKey + ".policy" );
      File.Delete( filepath );

    }




    /// <summary>
    /// 获取缓存策略对象
    /// </summary>
    /// <param name="cacheKey">缓存键</param>
    /// <returns>缓存策略对象</returns>
    public CachePolicy GetCachePolicy( string cacheKey )
    {
      var filepath = Path.Combine( CurrentDirectory, cacheKey + ".policy" );

      if ( File.Exists( filepath ) == false )
        return null;

      return CachePolicy.Parse( File.ReadAllText( filepath ) );
    }



    /// <summary>
    /// 设置缓存策略对象
    /// </summary>
    /// <param name="cacheKey"></param>
    /// <param name="cachePolicy"></param>
    public void SetCachePolicy( string cacheKey, CachePolicy cachePolicy )
    {
      var filepath = Path.Combine( CurrentDirectory, cacheKey + ".policy" );

      File.WriteAllText( filepath, cachePolicy.ToString() );
    }

    /// <summary>
    /// 释放所有资源，删除文件夹
    /// </summary>
    public void Dispose()
    {

      Directory.Delete( RootPath, true );

    }
  }
}
