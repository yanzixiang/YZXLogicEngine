using System;
using System.Threading;
using System.Xml.Serialization;
using System.Collections.Generic;

using Microsoft.Scripting.Hosting;

using IronPython.Hosting;
using IronPython.Runtime.Exceptions;

using Common.Threading;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting.Providers;
using IronPython.Runtime.Operations;

namespace YZXLogicEngine.Task
{
  /// <summary>
  /// IronPython任务
  /// </summary>
  public class IronPythonTask : YZXTask
  {
    [XmlIgnore]
    public ScriptEngine Engine;

    [XmlIgnore]
    public ScriptScope Scope;

    private ScriptSource scriptSource;
    private CompiledCode compiledCode;
    CodeContext codeContext = DefaultContext.Default;

    [XmlIgnore]
    public Timer ResumeTimer;
    public bool JMC = true;

    public string Path { get; set; }

    public IronPythonTask()
    {
    }

    public void InitR()
    {
      Engine = Python.CreateEngine();
      Scope = Engine.CreateScope();
      ICollection<string> sps = Engine.GetSearchPaths();
      sps.Add(@"D:\Program Files (x86)\Siemens\Automation\WinCC RT Advanced\IRONPYTHON");
      Engine.SetSearchPaths(sps);

      ThreadStart = new ControlledThreadStart(RunPY);

      ThreadController = new ThreadController(ThreadStart,null,Name);
    }

    /// <summary>
    /// 创建IronPython任务
    /// </summary>
    /// <param name="pyfile">任务文件</param>
    public IronPythonTask(string name, string path)
    {
      Name = name;
      Path = path;
      InitR();
    }

    [XmlIgnore]
    public double RunTimes { get; private set; }
    private void RunPY(ThreadController controller, object state)
    {
      Engine.SetTrace(OnTracebackReceived);
      //ObjectOperations operations = Engine.CreateOperations(Scope);
      

      while (true)
      {
        try
        {
          LastStartTime = DateTime.Now;

          if (FirstRun)
          {
            FirstRun = false;
          }

          PythonFunction RunOneTimeAction = (PythonFunction)GetVariable("RunOneTime");
          PythonCalls.Call(codeContext, RunOneTimeAction);

          RunTimes++;
          LastFinishTime = DateTime.Now;
        }catch(Exception ex)
        {

        }

      }
    }

    public void Resume(object state = null)
    {
      _dbgContinue.Set();
    }
    AutoResetEvent _dbgContinue = new AutoResetEvent(false);

    private TracebackDelegate OnTracebackReceived(TraceBackFrame frame, string result, object payload)
    {
      if (TracebackEvent != null)
      {
        bool NotMyCode = frame.f_code.co_filename != Path;

        if (JMC & NotMyCode)
        {
          Console.WriteLine("Skip");
          return OnTracebackReceived;
        }
        IPYTracebackEventArgs args = new IPYTracebackEventArgs();
        args.frame = frame;
        args.result = result;
        args.payload = payload;
        try
        {
          //Console.WriteLine(String.Format("{0},{1},{2}",frame,result,payload));
          TracebackEvent(this, args);
        }catch(Exception ex)
        {
          Console.WriteLine(ex);
        }
      }
      WaitInput();
      return OnTracebackReceived;
    }

    private void WaitInput()
    {
      //Console.WriteLine("StartWaitInput");
      if (monitors.Count > 0)
      {
        _dbgContinue.WaitOne();
      }
    }


    public event EventHandler<IPYTracebackEventArgs> TracebackEvent;
    public event EventHandler<YZXTaskExceptionEventArgs> ExceptionEvent;

    public void Reload()
    {

    }

    public override void Init()
    {
      try
      {
        scriptSource = Engine.CreateScriptSourceFromFile(Path);
        compiledCode = scriptSource.Compile();
        compiledCode.Execute(Scope);
        
        PythonFunction InitAction = (PythonFunction)GetVariable("Init");
        PythonCalls.Call(codeContext, InitAction);
      }
      catch (ImportException e)
      {
      }
    }

    public override bool RunOneTime()
    {
      try
      {
        LastStartTime = DateTime.Now;
        Engine.ExecuteFile(Path);
        LastFinishTime = DateTime.Now;
        return true;
      }catch(Exception ex)
      {
        YZXTaskExceptionEventArgs args = new YZXTaskExceptionEventArgs();
        args.exception = ex;
        ExceptionEvent(this,args);
      }
      return false;
    }

    public override void Reset()
    {
      string reset = Path.Replace(".py", ".reset.py");
      Engine.ExecuteFile(reset);
    }

    public object GetVariable(string name)
    {
      return Scope.GetVariable(name);
    }
    public T GetVariable<T>(string name)
    {
      return Scope.GetVariable<T>(name);
    }

    private List<IronPythonTaskMonitor> monitors = new List<IronPythonTaskMonitor>();
    public void AddMonitor(IronPythonTaskMonitor monitor)
    {
      monitors.Add(monitor);
    }

    public void RemoveMonitor(IronPythonTaskMonitor monitor)
    {
      monitors.Remove(monitor);
    }
  }
}