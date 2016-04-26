using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.IO;

namespace Ivony.Caching.Test
{
  /// <summary>
  /// MonitorTesting 的摘要说明
  /// </summary>
  [TestClass]
  public class DiskCacheTesting
  {

    /// <summary>
    ///获取或设置测试上下文，该上下文提供
    ///有关当前测试运行及其功能的信息。
    ///</summary>
    public TestContext TestContext { get; set; }

    #region 附加测试特性
    //
    // 编写测试时，可以使用以下附加特性: 
    //
    // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // 在运行每个测试之前，使用 TestInitialize 来运行代码
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // 在每个测试运行完之后，使用 TestCleanup 来运行代码
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion




    [TestMethod]
    public void DiskCacheTest()
    {


      Task.Run( async () =>
      {

        using ( var provider = new DiskCacheProvider( Path.Combine( Path.GetTempPath(), "Cache" ) ) )
        {
          var service = new CacheService( provider, CachePolicyProviders.Expires( TimeSpan.FromHours( 1 ) ) );

          for ( var i = 0; i < 1000; i++ )
          {
            var _value = await service.FetchOrAdd( "Test", ValueFactory );
            Assert.AreEqual( _value, value );
          }


          {
            await service.Remove( "Test" );
            value = null;

            var _value = await service.Fetch( "Test", "Test" );
            Assert.AreEqual( _value, "Test" );
            Assert.AreNotEqual( _value, value );
          }


          {
            await service.Set( "Test", ValueFactory );
            var _value = await service.FetchOrAdd( "Test", ValueFactory );
            Assert.AreEqual( _value, value );
          }

          {
            await service.Clear();
            var _value = await service.Fetch( "Test", "Test" );
            Assert.AreEqual( _value, "Test" );
            Assert.AreNotEqual( _value, value );
          }



        }
      } ).Wait();
    }



    private string value = null;


    private async Task<object> ValueFactory()
    {
      await Task.Yield();
      if ( value != null )
        Assert.Fail( "重复创建值" );
      return value = Path.GetRandomFileName();
    }
  }
}
