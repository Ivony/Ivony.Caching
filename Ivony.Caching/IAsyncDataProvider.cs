using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public interface IAsyncDataProvider
  {

    Task<object> Read( string key );

    Task Write( string key, object value );

    Task Clear();

  }
}
