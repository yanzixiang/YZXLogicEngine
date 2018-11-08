using System;
using System.Diagnostics;
using System.Threading;
#if NET462
using System.Threading.Tasks;
#endif
using System.Xml.Serialization;

using Common;
using Common.Threading;
using Extensions;

namespace YZXLogicEngine
{
  /// <summary>
  /// 任务执行的方式
  /// </summary>
  public enum YZXTaskRunType
  {
    TIMER,
    CONTINUE,
    EVENT,
    WaitForFinish
  }
  /// <summary>
  /// 任务
  /// 每个任务由一个线程执行
  /// 这个线程由线程控制器控制
  /// </summary>
  public abstract class YZXTask
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public object Tasklock = new object();

    [XmlIgnore]
    public ControlledThreadStart ThreadStart;

    [XmlIgnore]
    public ThreadController ThreadController;

    public YZXTaskRunType RunType = YZXTaskRunType.TIMER;
    public int Tick = 50;

    /// <summary>
    /// 启动线程
    /// </summary>
    public void StartThread()
    {
      ThreadController.Start();
      Running = true;
    }


    /// <summary>
    /// 是否第一次执行
    /// </summary>
    [XmlIgnore]
    public bool FirstRun { get;  set; }

    public bool RequestStop { get; set; }

    /// <summary>
    /// 是否正在执行
    /// </summary>
    [XmlIgnore]
    public bool Running { get; private set; }

    [XmlIgnore]
    public bool Finished { get; private set; }
    /// <summary>
    /// 上次开始执行时间
    /// </summary>
    [XmlIgnore]
    public DateTime LastStartTime { get; protected set; }

    /// <summary>
    /// 上次结束执行时间
    /// </summary>
    [XmlIgnore]
    public DateTime LastFinishTime { get; protected set; }

    [XmlIgnore]
    public TimeSpan LastRunTime
    {
      get
      {
        return LastStartTime - LastFinishTime;
      }
    }

    public Stopwatch RuntimeWatch = new Stopwatch();

    /// <summary>
    /// 初始化
    /// </summary>
    public abstract void Init();

    public void InitTask()
    {
      RequestStop = false;
      FirstRun = true;
      Running = false;

      Init();

    }

    /// <summary>
    /// 循环执行
    /// </summary>
    /// <returns>执行是否成功</returns>
    public abstract bool RunOneTime();

    /// <summary>
    /// 开始执行
    /// </summary>
    public void Start()
    {
      lock (Tasklock)
      {
        Running = true;
        Finished = false;
        LastStartTime = DateTime.Now;
#if NET462
        RuntimeWatch.Restart();
#endif
      }
    }

    /// <summary>
    /// 结束执行
    /// </summary>
    public void Finish()
    {
      lock (Tasklock)
      {
        FirstRun = false;
        Running = false;
        Finished = true;
        LastFinishTime = DateTime.Now;
        RuntimeWatch.Stop();
      }
    }

    public void RunOneTimeInTimer(object o = null)
    {
      lock (Tasklock)
      {

        if (RunType == YZXTaskRunType.WaitForFinish)
        {
          if (FirstRun)
          {
            Start();
            RunOneTime();
            Finish();
          }
          else
          {
            while (!Finished)
            {
            }
            Start();
            RunOneTime();
            Finish();
          }
        }
        else
        {
          Start();
          RunOneTime();
          Finish();
        }
      }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public abstract void Reset();

    public void ResetTask()
    {
      Running = false;
    }

    public void Stop()
    {

    }
    public string Serialize()
    {
      return this.SerializeObject();
    }

  }

  public enum YZXTaskTypes
  {
    IronPython = 1,
  }
}
