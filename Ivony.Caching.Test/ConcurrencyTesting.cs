using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace Ivony.Caching.Test
{
  [TestClass]
  public class ConcurrencyTesting
  {
    [TestMethod]
    public void MemoryCache()
    {
      using ( var provider = new MemoryCacheProvider( "Test" ).AsAsyncProvider() )
      {
        var cacheService = new AsyncCacheService( provider );


        var tasks = new List<Task>();


        //测试并发创建值
        for ( int i = 0; i < 1000; i++ )
        {
          Func<int, Task> task = async ( j ) =>
          {
            await Task.Yield();

            var value = await cacheService.FetchOrAdd( "Test", ValueFactory, new CachePolicy( DateTime.UtcNow.AddHours( 1 ) ) );
            Assert.AreEqual( value, _value );
          };

          tasks.Add( task( i ) );
        }

        Task.WaitAll( tasks.ToArray() );



        //测试从缓存读取
        for ( int i = 0; i < 1000; i++ )
        {
          Func<int, Task> task = async ( j ) =>
          {
            await Task.Yield();

            var value = await cacheService.FetchOrAdd( "Test", ValueFactory, new CachePolicy( DateTime.UtcNow.AddHours( 1 ) ) );
            Assert.AreEqual( value, _value );
          };

          tasks.Add( task( i ) );
        }
        Task.WaitAll( tasks.ToArray() );




        //清除缓存
        cacheService.Clear();
        _value = null;




        //测试创建新值
        for ( int i = 0; i < 1000; i++ )
        {
          Func<int, Task> task = async ( j ) =>
          {
            await Task.Yield();

            var value = await cacheService.FetchOrAdd( "Test", ValueFactory, new CachePolicy( DateTime.UtcNow.AddHours( 1 ) ) );
            Assert.AreEqual( value, _value );
          };

          tasks.Add( task( i ) );
        }
        Task.WaitAll( tasks.ToArray() );



        cacheService.Clear();
        {
          _value = null;
          var value = cacheService.FetchOrAdd( "Test", ValueFactory, new CachePolicy( DateTime.UtcNow.AddHours( 1 ) ) ).Result;

          Assert.IsNotNull( _value );
          Assert.AreEqual( _value, value );
        }
      }
    }



    [TestMethod]
    public void ExcepionTest()
    {

      using ( var provider = new MemoryCacheProvider( "Test" ).AsAsyncProvider() )
      {
        var cacheService = new AsyncCacheService( provider );


        Task.Run( async () =>
          {

            bool exception_catched = false;

            try
            {

              await cacheService.FetchOrAdd( "Test", ValueFactoryWithException, new CachePolicy( DateTime.UtcNow.AddHours( 1 ) ) );
            }
            catch
            {
              exception_catched = true;
            }

            Assert.IsTrue( exception_catched );

            var value = await cacheService.FetchOrAdd( "Test", ValueFactory, new CachePolicy( DateTime.UtcNow.AddHours( 1 ) ) );
            Assert.AreEqual( value, _value );


          }
        ).Wait();

      }
    }



    private volatile string _value = null;

    private async Task<string> ValueFactory()
    {

      await Task.Delay( 200 );

      if ( _value != null )
        Assert.Fail( "重复创建值" );

      return _value = Path.GetRandomFileName();
    }


    private async Task<string> ValueFactoryWithException()
    {

      await Task.Delay( 200 );
      throw new Exception();
    }


  }
}
