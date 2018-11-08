using System.Collections.Generic;

namespace YZXLogicEngine.UDT
{
  /// <summary>
  /// 任务状态
  /// </summary>
  public class YZXTaskStatus:YZXUDT
  {
    public override int UDTLength
    {
      get { return 25; }
    }

    public override Dictionary<string, string> Members {
      get{
        return new Dictionary<string, string>() {
        {"UpdateTimes","0"},
        {"ErrorTimes","4"},
        {"SuccessTimes","8"},
      }; 
      }
    }

    public YZXTask Task;
    public YZXTaskStatus()
    {
    }
    public YZXUint UpdateTimes { get; set; }
    public YZXUint ErrorTimes { get; set; }
    public YZXUint SuccessTimes { get; set; }

  }
}
