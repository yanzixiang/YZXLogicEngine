using System.Threading;

namespace YZXLogicEngine
{
  /// <summary>
  /// 位
  /// </summary>
  public class YZXBit
  {
    public YZXCPUMemory Mermory { get; private set; }//所属存储区域

    public YZXUnit Unit { get;  set; }          //映射到的模块
    public int UShortIndexInUnit{get; set;}      //映射到的模块中的字地址
    public int BitIndexInUnit{get; set;}      //映射到的模块中的位地址
    public bool Writeable { get; set; }

    private bool ValueBit;
    public bool Value{
      get {
        return ValueBit;
      }
      set{
        ValueBit = value;
      }
    }

    AutoResetEvent _dbgContinue = new AutoResetEvent(false);

    public YZXBit(YZXCPUMemory m, bool bit = false)
    {
      Mermory = m;
      Value = bit;
      Writeable = true;//默认可写
    }

    public YZXBit Set()
    {
      Value = true;
      return this;
    }

    public YZXBit Reset()
    {
      Value = false;
      return this;
    }

    public YZXBit revert()
    {
      Value = !Value;
      return this;
    }

    #region 类型转换
    static public implicit operator bool(YZXBit b)
    {
      return b.Value;
    }
    #endregion 类型转换

    public override string ToString()
    {
      return Value ? "1" : "0";
    }
  }
}
