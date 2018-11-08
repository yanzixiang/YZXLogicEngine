using System;
using System.Threading;
using System.Threading.Tasks;

namespace YZXLogicEngine.Morse
{
  public class ExceptionEventArgs : EventArgs
  {
    public Exception exception;
    public ExceptionEventArgs(Exception ex)
    {
      exception = ex;
    }
  }
  public delegate bool getCurrentState();
  public class MorseReceiver
  {
    public int ReadTick = 100;
    public int TickTime = 1000;
    public int diTicks = 1;
    public int daTicks = 3;
    /// <summary>
    /// DI 和 DA 之间间隔
    /// </summary>
    public int ibaTicks = 1;
    public int charSpaceTicks = 3;
    public int wordSpaceTicks = 7;

    public Timer ReadTimer;
    public DateTime LastOnTime;
    public DateTime LastOffTime;

    private bool currentState;
    public bool CurrentState
    {
      get
      {
        return currentState;
      }
      set
      {
        if(currentState != value) {
          if (value)
          {
            // 0 ----> 1
            LastOnTime = DateTime.Now;
          }
          else
          {
            // 1 ----> 0
            LastOffTime = DateTime.Now;
            TimeSpan onTime = LastOffTime - LastOnTime;
            double tms = onTime.TotalMilliseconds;
            double diTime = diTicks * TickTime;
            double daTime = daTicks * TickTime;
            if(tms > daTime - diTime)
            {
              //da
              ToggleEventArgs tea = new ToggleEventArgs(tms);
              RaiseDa(tea);
            }
            if((diTime / 2 < tms) & (tms < daTime - diTime))
            {
              //di
              ToggleEventArgs tea = new ToggleEventArgs(tms);
              RaiseDi(tea);
            }
          }
          currentState = value;
        }
        else
        {
          if (value)
          {
            // 1 keep
          }
          else
          {
            // 0 keep
          }
        }
      }
    }
    getCurrentState ReadCurrentStateDelegate;

    System.Threading.Tasks.Task CurrentTask = null;
    public bool Sending
    {
      get
      {
        return Sending;
      }
    }

    public delegate void DiDaReceivedHandler(MorseReceiver receiver, ToggleEventArgs e);
    public delegate void ReadExcepitonEventHandler(MorseReceiver sender, EventArgs e);
    public event DiDaReceivedHandler Di;
    public event DiDaReceivedHandler Da;
    public event DiDaReceivedHandler Space;
    public event ReadExcepitonEventHandler Read;
    public void RaiseDi(ToggleEventArgs e)
    {
      if (Di != null)
        Di(this, e);
    }
    public void RaiseDa(ToggleEventArgs e)
    {
      if (Da != null)
        Da(this, e);
    }

    public void ReadCurrentState(object state)
    {
      try
      {
        bool result = ReadCurrentStateDelegate();
        CurrentState = result;
      }catch(Exception ex)
      {
      }
    }

    
    public void Start(getCurrentState gcs)
    {
        ReadCurrentStateDelegate = gcs;
        ReadTimer = new Timer(new TimerCallback(ReadCurrentState), null, 0, ReadTick);
    }

    public void Stop()
    {
      if(ReadTimer != null)
      {
        ReadTimer.Change(-1, -1);
      }
    }
  }
}
