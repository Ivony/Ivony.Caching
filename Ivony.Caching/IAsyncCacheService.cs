using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public interface IAsyncCacheService
  {

    Task<T> Fetch<T>( string cacheKey, T defaultValue = default( T ), CancellationToken cancellationToken = default( CancellationToken ) );

    Task<T> FetchOrAdd<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy cachePolicy, CancellationToken cancellationToken = default( CancellationToken ) );

    Task Set<T>( string cacheKey, Func<Task<T>> valueFactory, CachePolicy cachePolicy, CancellationToken cancellationToken = default( CancellationToken ) );

    Task ClearCache();

    Task ClearCache( string cacheKey );
  }
}
