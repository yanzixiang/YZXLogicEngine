using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using Common;
using Extensions;

using YZXMessaging;
using YZXLogicEngine;
using YZXLogicEngine.Task;
using YZXLogicEngine.Unit;
using YZXLogicEngine.UDT;

namespace YZXLogicEngine
{
  /// <summary>
  /// CPU
  /// 针对设备编程
  /// </summary>
  public partial class YZXCPU
  {
    /// <summary>
    /// 任务
    /// </summary>
    public Dictionary<string, YZXTask> Tasks { get; set; }
    private Dictionary<string, Timer> Timers;

    public void StartTasks()
    {
      foreach (YZXTask task in Tasks.Values)
      {
        Console.WriteLine("StartTask",task.Name);
        //task.Value.StartThread();
        StartTask(task);
      }
    }

    public void StartTask(object obj)
    {
      YZXTask task = (YZXTask)obj;
      StartTask(task);
    }

    public void StartTask(YZXTask task)
    {
      switch (task.RunType)
      {
        case YZXTaskRunType.TIMER:
          StartTimerTask(task);
          break;
        case YZXTaskRunType.WaitForFinish:
          task.InitTask();
          StartWaitForFinishTask(task);
          break;
        case YZXTaskRunType.CONTINUE:
          task.InitTask();
          task.StartThread();
          break;
      }
    }

    private void StartTimerTask(YZXTask task)
    {
      TimerCallback scannerupdatetimercallback =
          new TimerCallback(task.RunOneTimeInTimer);

      Timers[task.Name] = new Timer(scannerupdatetimercallback, task, 0, task.Tick);
    }

    private void StartWaitForFinishTask(YZXTask task)
    {
      while (!task.RequestStop)
      {
        task.RunOneTimeInTimer();
        Thread.Sleep(50);
      }
    }

    public void StopTasks()
    {
    }

    #region 强制垃圾回收
    public bool GCRun()
    {
      try
      {
        long x = 0, y1 = 0, y2 = 0, y3 = 0;

        Process process = Process.GetCurrentProcess();

        y1 = process.PrivateMemorySize64;

        GC.Collect();

        y2 = process.PrivateMemorySize64;

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
      return true;
    }
    public void AddGCTask()
    {
      CSharpTask gctask = new CSharpTask();
      gctask.Name = "GC";
      gctask.RunOneTimeAction = GCRun;

      gctask.RunType = YZXTaskRunType.TIMER;
      gctask.Tick = 105;

      Tasks["GC"] = gctask;
    }
    #endregion 

    public bool HasTask(string TaskName)
    {
      if (Tasks.ContainsKey(TaskName))
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public YZXTask GetTask(string TaskName)
    {
      if (Tasks.ContainsKey(TaskName))
      {
        return Tasks[TaskName];
      }
      else
      {
        return null;
      }
    }

    public YZXTask GetTask(int id)
    {
      foreach(YZXTask task in Tasks.Values)
      {
        if(task.Id == id)
        {
          return task;
        }
      }
      return null;
    }
    public IronPythonTask AddIronPythonTask(string TaskName, string PYPath)
    {
      IronPythonTask task = new IronPythonTask(TaskName, PYPath);

      task.Scope.SetVariable("self", this);
      task.Name = TaskName;
      task.RunType = YZXTaskRunType.CONTINUE;
      //task.RunType = YZXTaskRunType.WaitForFinish;

      AddTask(task);
      return task;
    }

    public void AddTask(YZXTask task)
    {
      Tasks[task.Name] = task;
      task.Id = Tasks.Count;
    }
  }
}