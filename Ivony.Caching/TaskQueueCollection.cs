using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{

  /// <summary>
  /// 任务队列容器，协助实现任务排队执行调度。
  /// </summary>
  internal sealed class TaskQueueCollection
  {


    private Dictionary<string, Task> _collection = new Dictionary<string, Task>();
    private object _sync = new object();


    /// <summary>
    /// 获取或添加
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="taskFactory">一个创建要添加的 Task 的方法</param>
    /// <returns>已存在的或者被添加进去的 Task</returns>

    public Task GetOrAdd( string key, Func<Task> taskFactory )
    {
      Task task;
      lock ( _sync )
      {
        if ( _collection.TryGetValue( key, out task ) == false )
        {
          _collection.Add( key, task = taskFactory() );
        }
      }
      return task;
    }


    /// <summary>
    /// 等待并移除，等待一个任务执行完毕，如果执行完毕后这个任务是当前任务，则移除。
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="task">等待执行并移除的任务</param>
    /// <returns></returns>
    public async Task WaitAndRemove( string key, Task task )
    {
      try
      {
        await task;
      }
      finally
      {
        lock ( _sync )
        {
          Task current;
          if ( _collection.TryGetValue( key, out current ) && current == task )
            _collection.Remove( key );
        }
      }
    }


    /// <summary>
    /// 尝试从列表中获取一个任务
    /// </summary>
    /// <param name="key">键</param>
    /// <param name="task">列表中的任务，如果有的话</param>
    /// <returns>是否成功获取一个任务</returns>
    public bool TryGetValue( string key, out Task task )
    {
      lock ( _sync )
      {
        return _collection.TryGetValue( key, out task );
      }
    }
  }
}
