using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ivony.Caching
{
  public sealed class TaskManager
  {


    private Dictionary<string, TaskItem> _dictionary;

    private object _sync = new object();


    public sealed class TaskItem : IDisposable
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
          if ( _manager == null )
            throw new ObjectDisposedException( "TaskItem" );


          TaskItem item;
          if ( _manager._dictionary.TryGetValue( _key, out item ) == false )
            return null;

          if ( item != this )
            return null;


          if ( _task == null )
            _task = _taskFactory();

          Task.Run( async () =>//任务执行完成，则立即抛弃当前项。
          {
            using ( this )
            {
              await _task;
            }
          } );

          return _task;

        }
      }


      /// <summary>
      /// 释放所有资源，并从管理器中移除
      /// </summary>
      public void Dispose()
      {
        _task = null;
        _manager = null;
        _taskFactory = null;
        _manager._dictionary.Remove( _key );
      }



      /// <summary>
      /// 获取任务项
      /// </summary>
      public Task Task { get { return GetTask(); } }
    }

  }

}
