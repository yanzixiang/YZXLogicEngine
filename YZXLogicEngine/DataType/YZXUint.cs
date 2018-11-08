using System;

using Extensions;

namespace YZXLogicEngine
{
  /// <summary>
  /// 无符号双字
  /// 32位
  /// 0 ~ 2^32 - 1
  /// </summary>
  public class YZXUint
  {
    public YZXCPUMemory Mermory { get; private set; }
    public int UintIndex { get; private set; }

    public uint Value
    {
      get
      {
        uint value = (uint)this;
        return value;
      }
      set
      {
        bool[] vbits = value.ToboolArray();
        for (int i = 0; i < 31; i++)
        {
          this[i].Value = vbits[i];
        }
      }
    }

    public YZXUint(YZXCPUMemory m,int uintIndex)
    {
      Mermory = m;
      UintIndex = uintIndex;
    }

    #region 类型转换
    public static explicit operator uint(YZXUint dw)
    {
      uint i16 = 0;
      uint adder = 1;
      for (short i = 0; i < 31; i++)
      {
        if (dw[i])
        {
          i16 += adder;
        }
        adder *= 2;
      }
      return i16;
    }
    #endregion 类型转换

    public YZXBit this[int BIndex]
    {
      get
      {
        if (!BIndex.InRange(0,31,true))
        {
          throw new InvalidOperationException();
        }
        int realIndex = UintIndex * 8 + BIndex;
        YZXBit bit = Mermory.B[realIndex];
        return bit;
      }
    }

    public override string ToString()
    {
      string s = "";
      return s;
    }
  }
}
