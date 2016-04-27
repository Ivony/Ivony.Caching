using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ivony.Caching.Test
{
  [TestClass]
  public class TaskTesting
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
    public void YieldTest()
    {
      Task.Run( async () =>
      {


        string output = null;

        Func<Task> foo = async () =>
        {
          output = "before yield";
          await Task.Yield();
          output = "after yield";

        };

        var task = foo();

        await Task.Delay( TimeSpan.FromSeconds( 1 ) );
        Assert.AreEqual( output, "after yield" );
      } ).Wait();
    }



    [TestMethod]
    public void PromotorTest()
    {
      Task.Run( async () =>
      {


        var output = (string) null;
        var promotor = new TaskCompletionSource<object>();

        Func<Task> foo = async () =>
        {
          output = "before promotor";
          await promotor.Task;
          output = "after promotor";

        };

        var task = foo();

        await Task.Delay( TimeSpan.FromSeconds( 1 ) );
        Assert.AreEqual( output, "before promotor" );

        promotor.SetResult( null );
        await Task.Delay( TimeSpan.FromSeconds( 1 ) );
        Assert.AreEqual( output, "after promotor" );
      } ).Wait();
    }
  }
}
