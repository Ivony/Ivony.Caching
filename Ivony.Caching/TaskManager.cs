using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{


  /// <summary>
  /// 对 Task 进行队列和其他管理的服务
  /// </summary>
  public sealed class TaskManager
  {


    private readonly ConcurrentDictionary<string, TaskItem> _dictionary = new ConcurrentDictionary<string, TaskItem>();



    /// <summary>
    /// 获取当前正在执行的任务，若当前没有正在执行的任务，则创建一个新的。
    /// </summary>
    /// <param name="key"></param>
    /// <param name="taskFactory"></param>
    /// <returns></returns>
    public Task GetOrAdd( string key, Func<Task> taskFactory )
    {

      return _dictionary.GetOrAdd( key, new TaskItem( key, this, taskFactory ) ).Task;

    }


    /// <summary>
    /// 一个任务项
    /// </summary>
    private sealed class TaskItem : IDisposable
    {

      private string _key;
      private TaskManager _manager;
      private Func<Task> _taskFactory;

      private object _sync;
      private Task _task;


      public TaskItem( string key, TaskManager manager, Func<Task> taskFactory )
      {
        this._key = key;
        this._manager = manager;
        this._taskFactory = taskFactory;

        _sync = new object();
        _task = null;
      }


      private Task GetTask()
      {
        lock ( _sync )
        {
          if ( _task == null )
            _task = _taskFactory();

          _task.GetAwaiter().OnCompleted( () => this.Dispose() );//任务执行完成，则立即抛弃当前项。

          return _task;
        }
      }


      /// <summary>
      /// 释放所有资源，并从管理器中移除
      /// </summary>
      public void Dispose()
      {

        lock ( _sync )
        {
          if ( _manager == null )
            return;

          TaskItem item;
          _manager._dictionary.TryRemove( _key, out item );
          _manager = null;
        }
      }




      /// <summary>
      /// 获取任务项
      /// </summary>
      public Task Task { get { return GetTask(); } }
    }

  }

}
