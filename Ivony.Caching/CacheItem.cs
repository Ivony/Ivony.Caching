namespace Ivony.Caching
{
  public class CacheItem
  {

    public CacheItem( string key, object value, CachePolicy cachePolicy )
    {
      CacheKey = key;
      Value = value;
      CachePolicy = cachePolicy;
    }

    public string CacheKey { get; }

    public object Value { get; }

    public CachePolicy CachePolicy { get; }

  }
}