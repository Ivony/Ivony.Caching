using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public class CacheService
  {


    private static readonly Dictionary<string, Task> _tasks = new Dictionary<string, Task>();
    private static readonly Dictionary<string, object> _values = new Dictionary<string, object>();

    private static readonly object _sync = new object();


    public static async Task<T> FetchOrAddAsync<T>( string cacheKey, Func<string, Task<T>> valueFactory, CancellationToken cancellationToken = default(CancellationToken) )
    {

      Task task = null;
      object value = null;

      lock ( _sync )
      {
        if ( _tasks.TryGetValue( cacheKey, out task ) == false )
        {
          if ( _values.TryGetValue( cacheKey, out value ) )
          {
            if ( value is T )
              return (T) value;
          }

          _tasks.Add( cacheKey, task = FetchValueAsync( cacheKey, valueFactory ) );
        }
      }


      await task;
      return await FetchOrAddAsync( cacheKey, valueFactory, cancellationToken );
    }

    private static async Task FetchValueAsync<T>( string cacheKey, Func<string, Task<T>> valueFactory )
    {
      await Task.Yield();
      _values[cacheKey] = await valueFactory( cacheKey );
    }
  }
}
