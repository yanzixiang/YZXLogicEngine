using System;

namespace YZXLogicEngine.Task
{
    /// <summary>
    /// CSharpTask任务
    /// </summary>
  public class CSharpTask : YZXTask
  {
    public Action InitAction;
    public Func<bool> RunOneTimeAction;
    public Action ResetAction;

    public CSharpTask()
    {
      InitAction = (() => { });
      ResetAction = (() => { });

      RunOneTimeAction = () => { 
        return true; 
      };

    }
    public CSharpTask(Action init,Func<bool> run,Action reset)
    {
      InitAction = init;
      RunOneTimeAction = run;
      ResetAction = reset;
    }
    public void Reload()
    {

    }

    public override void Init()
    {
      InitAction();
    }

    public override bool RunOneTime()
    {
      return RunOneTimeAction();
    }

    public override void Reset()
    {
      ResetAction();
    }
  }
}
