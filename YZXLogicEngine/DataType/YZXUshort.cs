using System;

using Extensions;

namespace YZXLogicEngine
{
  /// <summary>
  /// 无符号字
  /// 16位
  /// 0 ~ 65535
  /// </summary>
  public class YZXUshort
  {
    public YZXCPUMemory Mermory { get; private set; }//所属存储区域

    public YZXUnit Unit { get; set; }          //映射到的模块
    public ushort UShortIndexInUnit { get; set; }      //映射到的模块中的字地址

    public ushort UshortIndex { get; private set; }

    public bool Writeable { get; set; }

    public ushort Value
    {
      get
      {
        ushort i16 = this;
        return i16;
      }
      set
      {
        //设备每一个位
        bool[] vbits = value.ToBoolArray();
        for (int i = 0; i < 16; i++)
        {
          this[i].Value = vbits[i];
        }

        if (Unit != null)
        {
          if (Unit is YZXWriteableUnit)
          {
            YZXWriteableUnit unit = Unit as YZXWriteableUnit;
            unit.WriteUshort(UshortIndex, value);
          }
          else
          {
            string s = string.Format("{0} is not a Writeable Unit", Unit.Name);
            throw new InvalidOperationException(s);
          }
        }
      }
    }

    public YZXUshort(YZXCPUMemory m, ushort ushortIndex)
    {
      Mermory = m;
      UshortIndex = ushortIndex;
      Writeable = true;//默认可写
    }

    #region 类型转换
    public static implicit operator ushort(YZXUshort w)
    {
      ushort i16 = 0;
      ushort adder = 1;
      for (short i = 0; i < 16; i++)
      {
        if (w[i])
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
        if (BIndex.InRange(0, 15,true))
        {
          int realIndex = UshortIndex * 16 + BIndex;
          YZXBit bit = Mermory.B[realIndex];
          return bit;
        }
        throw new InvalidOperationException();
      }
    }

    public override string ToString()
    {
      string s = string.Format("{0}-{1}-{2}", Mermory.Name, UshortIndex, Value);
      return s;
    }
  }
}
