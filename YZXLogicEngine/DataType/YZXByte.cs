using System;

using Extensions;

namespace YZXLogicEngine
{
  /// <summary>
  /// 无符号字节
  /// 8位
  /// 0 ~ 255
  /// </summary>
  public class YZXByte
  {
    public YZXCPUMemory Mermory { get; private set; }//所属存储区域
    public int ByteIndex { get; private set; }

    public byte Value{
      get {
        byte b = 1;
        return b;
      }
      set{
      }
    }

    public YZXByte(YZXCPUMemory m,int byteIndex)
    {
      Mermory = m;
      ByteIndex = byteIndex;
    }
    #region 类型转换
    static public implicit operator byte(YZXByte b)
    {
      byte b1 = 0;
      return b1;
    }
    #endregion 类型转换

    public YZXBit this[int BIndex]
    {
      get
      {
        if (!BIndex.InRange(0,7,true))
        {
          throw new InvalidOperationException();
        }

        int realIndex = ByteIndex * 8 + BIndex;
        YZXBit bit = Mermory.B[realIndex];
        return bit;
      }
    }

    public override string ToString()
    {
      return ((int)this).ToString();
    }
  }
}
