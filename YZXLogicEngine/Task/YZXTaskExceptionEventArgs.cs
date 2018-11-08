using System;

using IronPython.Runtime.Exceptions;

namespace YZXLogicEngine.Task
{
  public class YZXTaskExceptionEventArgs : EventArgs
  {
    public Exception exception;

    public YZXTaskExceptionEventArgs()
    {

    }
  }
}
