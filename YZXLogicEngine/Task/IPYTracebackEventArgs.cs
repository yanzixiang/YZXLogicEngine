using System;

using IronPython.Runtime.Exceptions;

namespace YZXLogicEngine.Task
{
  public class IPYTracebackEventArgs : EventArgs
  {
    public TraceBackFrame frame;
    public string result;
    public object payload;

    public IPYTracebackEventArgs()
    {

    }
  }
}
