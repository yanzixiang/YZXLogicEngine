using System.Collections.Generic;

namespace YZXLogicEngine.UDT
{
  /// <summary>
  /// 气缸
  /// </summary>
  public class YZXCylinder:YZXUDT
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
       
    public YZXUnit Unit;
    public YZXCylinder()
    {
    }

    public YZXBit UpdateTimes { get; set; }
    public YZXBit ErrorTimes { get; set; }
    public YZXBit SuccessTimes { get; set; }

  }

  public enum CylinderType
  {
    ShenSuo = 0,
    ShangXia = 1,
    SongJin= 2,
  }
}
