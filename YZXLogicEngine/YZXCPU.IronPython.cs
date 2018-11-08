using System;
using System.Collections.Generic;

using Microsoft.Scripting.Hosting;
using IronPython.Hosting;

namespace YZXLogicEngine
{
  public partial class YZXCPU
  {
    public ScriptEngine Engine;

    public ScriptScope Scope;

    private ScriptSource scriptSource;
    private CompiledCode compiledCode;

    public bool PYInited { get; set; } = false;

    public void InitPY(bool force = false)
    {
      if (!PYInited | force) {
        Engine = Python.CreateEngine();
        Scope = Engine.CreateScope();
        ICollection<string> sps = Engine.GetSearchPaths();
        sps.Add(@"D:\Program Files (x86)\Siemens\Automation\WinCC RT Advanced\IRONPYTHON");
        Engine.SetSearchPaths(sps);

        PYInited = true;
      }
    }

    #region IPY变量
    public Dictionary<string, object> Variables = new Dictionary<string, object>();
    public void UpdateVariables()
    {
      foreach (KeyValuePair<string, object> v in Variables)
      {
        string tag = v.Key;
        object value = v.Value;
        Scope.SetVariable(tag, value);
      }
    }
    public void SetVariable(string tag, object value)
    {
      Variables[tag] = value;
    }
    #endregion IPY变量

    public void RunPY(string Path)
    {
      try
      {
        scriptSource = Engine.CreateScriptSourceFromFile(Path);
        compiledCode = scriptSource.Compile();
        compiledCode.Execute(Scope);
      }catch(Exception ex)
      {

      }
    }
  }
}
