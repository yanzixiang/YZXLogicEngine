using System;

using Common.Threading;

namespace YZXLogicEngine.Task
{
  /// <summary>
  /// 计时器
  /// </summary>
  public class YZXTimerTask:YZXTask
  {
    private bool buffer;
    public bool In { get; private set; }
    public bool Out { get; private set; }
    public bool OffOut { get; private set; }

    protected Func<bool?> CheckFunc;
    protected int OnLastMs;
    protected int OffLastMs;

    /// <summary>
    /// 最后检查时间
    /// </summary>
    public DateTime LastCheckTime { get; private set; }

    /// <summary>
    /// 最后On时间
    /// </summary>
    public DateTime LastOnTime { get; private set; }

    /// <summary>
    /// 0 -> 1
    /// </summary>
    public DateTime UpTime { get; private set; }

    /// <summary>
    /// In 保持为 1 时间
    /// </summary>
    public TimeSpan OnFor
    {
      get
      {
        return LastOnTime - UpTime;
      }
    }

    /// <summary>
    /// 最后Off时间
    /// </summary>
    public DateTime LastOffTime { get; private set; }

    /// <summary>
    /// 1 -> 0
    /// </summary>
    public DateTime DownTime { get; private set; }

    /// <summary>
    /// In 保持为 1 时间
    /// </summary>
    public TimeSpan OffFor
    {
      get
      {
        return LastOffTime - DownTime;
      }
    }

    public YZXTimerTask(string name)
    {
      Name = name;
    }

    /// <summary>
    /// 计时器 
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="check">检查方法</param>
    /// <param name="lastMs">持续时间</param>
    /// <param name="offLast">反扶持时间</param>
    public YZXTimerTask(string name,Func<bool?> check,int lastMs = 1000,int offLast = 1000)
    {
      Name = name;
      CheckFunc = check;
      OnLastMs = lastMs;
      OffLastMs = offLast;

      ThreadStart = new ControlledThreadStart(RunCheck);

      ThreadController = new ThreadController(ThreadStart,null,Name);

      StartThread();
    }

    protected void RunCheck(ThreadController controller, object state)
    {
      while (true)
      {
        try
        {
          RunOneTime();
        }
        catch(Exception ex)
        {
        }
      }
    }

    public override void Init()
    {
    }

    public override bool RunOneTime()
    {
      try
      {
        bool? current = CheckFunc();
        if(current == null)
        {
          return false;
        }

        In = (bool)current;
        LastCheckTime = DateTime.Now;
        if (In)
        {
          //In 为 1

          LastOnTime = DateTime.Now;
          OffOut = false;
          if (buffer)
          {
            if (OnFor.TotalMilliseconds > OnLastMs)
            {
              Out = true;
              OutChanged?.Invoke(this, null);
            }
          }
          else
          {
            UpTime = DateTime.Now;
            buffer = true;
            OffOutChanged?.Invoke(this, null);
          }
        }
        else
        {
          //In 为 0

          LastOffTime = DateTime.Now;
          Out = false;
          if (buffer)
          {
            buffer = false;
            DownTime = DateTime.Now;
            OutChanged?.Invoke(this, null);
          }
          else
          {
            if(OffFor.TotalMilliseconds > OffLastMs)
            {
              OffOut = true;
              OffOutChanged?.Invoke(this, null);
            }
          }
        }
      }catch(Exception ex)
      {
      }
      return true;
    }

    public override void Reset()
    {
    }

    public event EventHandler OutChanged;
    public event EventHandler OffOutChanged;
  }
}
