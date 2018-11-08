using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Common.Threading;

using Extensions;

using YZXLogicEngine.Unit;
using YZXLogicEngine.UDT;

namespace YZXLogicEngine
{
  /// <summary>
  /// 模块
  /// 
  /// 映射到内存区域
  /// CPU对内存区域的读写操作被转发到这里
  /// 
  /// 模块本身不知道自己被映射到了CPU的哪个内存区域
  /// 
  /// 如果一个模块被映射到多个内存区域，
  /// 会产生重复的读操作，
  /// 这样做也没有什么意义
  /// </summary>
  public abstract class YZXUnit
  {
    public string Name { get; set; }
    public ushort Length { get; set; }

    protected List<bool> bits;

    [XmlIgnore]
    public YZXUnitStatus Status;

    /// <summary>
    /// 写操作的锁
    /// </summary>
    public OrderedLock ReadLock;

    [XmlIgnore]
    public OrderedLock UpdateLock;

    /// <summary>
    /// 是否正在更新值
    /// </summary>
    public bool Updating = false;

    public YZXUnit()
    {
    }

    public YZXUnit(string name, ushort length = 100) :
      base()
    {
      Name = name;
      Length = length;
      bits = new List<bool>(Length * 16);
      Status = new YZXUnitStatus();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// CPU对内存区域的读操作
    /// 定时更新整个模块的所有区域
    /// </summary>
    /// <returns></returns>
    public abstract List<bool> UpdateBits();

    public void RecodeUpdatetimes() {
        Status.UpdateTimes.Value = Status.UpdateTimes.Value + 1;
    }

    public void RecodeSuccessTimes()
    {
        Status.SuccessTimes.Value = Status.SuccessTimes.Value + 1;
    }

    public void RecodeErrorTimes()
    {
        Status.ErrorTimes.Value = Status.ErrorTimes.Value + 1;
    }
    public void RecodeErrorTimes(Exception ex)
    {
      RecodeErrorTimes();
    }

    public string Serialize(){
      return this.SerializeObject();
    }

    //public abstract YZXUnit Deserialize(string s);

    public static YZXUnit Deserialize(YZXUnitTypes t, string xml)
    {
      YZXUnit unit = new RandomUnit("random");
      switch (t)
      {
        case YZXUnitTypes.Random:
          //unit = RandomUnit.Deserialize(xml);
          break;
        case YZXUnitTypes.ModbusTCP:
          //unit = ModbusTCPUnit.Deserialize(xml);
          break;
        case YZXUnitTypes.Snap7ClientDB:
          //unit = Snap7ClientDBUnit.Deserialize(xml);
          break;
      }
      return unit;
    }
  }

  public enum YZXUnitTypes
  {
    Random = 1,
    ModbusTCP = 2,
    Snap7ClientDB = 3,
  }
}