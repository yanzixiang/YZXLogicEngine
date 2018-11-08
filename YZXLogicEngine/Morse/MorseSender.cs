using System;
using System.Threading;
using System.Threading.Tasks;

namespace YZXLogicEngine.Morse
{
  public class MorseSender
  {
    public int TickTime = 1000;
    public int diTicks = 1;
    public int daTicks = 3;
    /// <summary>
    /// DI 和 DA 之间间隔
    /// </summary>
    public int ibaTicks = 1;
    public int charSpaceTicks = 3;
    public int wordSpaceTicks = 7;

    System.Threading.Tasks.Task CurrentTask = null;
    public bool Sending
    {
      get
      {
        return Sending;
      }
    }

    public delegate void ToggleEventHandler(MorseSender sender, ToggleEventArgs e);
    public event ToggleEventHandler UP;
    public event ToggleEventHandler DOWN;
    public void RaiseUP(ToggleEventArgs e)
    {
      if (UP != null)
        UP(this, e);
    }
    public void RaiseDOWN(ToggleEventArgs e)
    {
      if (UP != null)
        DOWN(this, e);
    }

    public void Loop(string s)
    {
      do
      {
        Send(s);
      } while (true);
    }

    public void Send(string s)
    {
      string morse = MorseConvert.word2morse(s);

      Console.WriteLine(morse);

      CurrentTask = System.Threading.Tasks.Task.Run(() => {
        foreach(char dida in morse)
        {
          ToggleEventArgs tea = new ToggleEventArgs(1);
          switch (dida)
          {
            case '.':
              tea.count = diTicks;
              RaiseUP(tea);
              tea.count = ibaTicks;
              RaiseDOWN(tea);
              break;
            case '-':
              tea.count = daTicks;
              RaiseUP(tea);
              tea.count = ibaTicks;
              RaiseDOWN(tea);
              break;
            case ' ':
              RaiseDOWN(tea);
              break;
          }
        }
      });
    }
  }
}
