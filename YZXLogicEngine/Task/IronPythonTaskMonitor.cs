namespace YZXLogicEngine.Task
{
  public interface IronPythonTaskMonitor
  {
    void ConnectToTask(IronPythonTask task);
    void DisconnectFromTask();
  }
}
