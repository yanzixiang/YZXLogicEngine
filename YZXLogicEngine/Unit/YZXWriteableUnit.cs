using System.Collections.Generic;

using Common.Threading;

namespace YZXLogicEngine
{
  /// <summary>
  /// 可写模块
  /// 写操作是定时写
  /// 还是在逻辑执行时写
  /// </summary>
  public abstract class YZXWriteableUnit:YZXUnit
  {
    /// <summary>
    /// 写操作的锁
    /// </summary>
    public OrderedLock WriteLock;

    public bool Writing = false;

    public YZXWriteableUnit()
    {
    }

    public YZXWriteableUnit(string name, ushort length = 100):
      base(name,length)
    {
      WriteLock = new OrderedLock(name + "-WriteLock");
    }

    public abstract bool WriteB(ushort wIndex, byte bIndex, bool v);
    public abstract bool WriteUshort(ushort wIndex, ushort i);
    public abstract bool WriteD(ushort dwIndex, int d);

    public abstract bool WriteBits(List<bool> bits);   
  }
}
