using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ivony.Caching.TestConsole
{
  static class Program
  {
    static void Main( string[] args )
    {

      var provider = new MemoryCacheProvider( "Test" ).AsAsyncProvider();
      var cacheService = new AsyncCacheService( provider );



      var tasks = new List<Task>();


      for ( int i = 0; i < 20; i++ )
      {
        Func<int, Task> task = async ( j ) =>
         {
           await Task.Yield();

           Console.WriteLine( "task {0} in thread {1} beginning", j, Thread.CurrentThread.ManagedThreadId );
           var value = await cacheService.FetchOrAdd( "Test", CreateObject, new CachePolicy( DateTime.UtcNow.AddHours( 1 ) ) );
           Console.WriteLine( "task {0} in thread {1} completed", j, Thread.CurrentThread.ManagedThreadId );
         };

        tasks.Add( task( i ) );
      }

      Task.WaitAll( tasks.ToArray() );

      Console.ReadKey();
    }

    private static async Task<object> CreateObject()
    {

      await Task.Yield();

      for ( int i = 0; i < 10; i++ )
      {
        await Task.Delay( 100 );
        Console.WriteLine( i );
      }

      return new object();

    }
  }
}
