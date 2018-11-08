using System.Collections.Generic;

namespace YZXLogicEngine.UDT
{
  /// <summary>
  /// CPU状态
  /// </summary>
  public class YZXCPUStatus:YZXUDT
  {

    public override int UDTLength
    {
      get { return 25; }
    }

    public override Dictionary<string, string> Members {
      get{
        return new Dictionary<string, string>() {
        {"GCTimes","0"},
        {"ErrorTimes","4"},
        {"SuccessTimes","8"},
      }; 
      }
    }
       


    public YZXUnit Unit;
    public YZXCPUStatus()
    {
    }

    public YZXUint GCTimes { get; set; }
    public YZXUint ErrorTimes { get; set; }
    public YZXUint SuccessTimes { get; set; }

  }
}
