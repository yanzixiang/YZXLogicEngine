using System;
using System.Collections.Generic;
using System.Reflection;

using YZXLogicEngine.DataType;

namespace YZXLogicEngine.UDT
{
  /// <summary>
  /// 自定义变量类型
  /// </summary>
  public abstract class YZXUDT:YZXCPUMemory
  {
    public abstract int UDTLength { get; }

    public static int GetUDTLength(YZXUDTTypes udttype)
    {
      switch (udttype)
      {
        case YZXUDTTypes.UnitStatus:
          YZXUnitStatus status = new YZXUnitStatus();
          return status.UDTLength;
      }
      return 0;
    }

    public abstract Dictionary<string, string> Members { get; }


    public YZXAddress AddressBase = new YZXAddress();

    public YZXUDT()
    {

    }

    public void BuildMemberAddress()
    {
      foreach (KeyValuePair<string, string> member in Members)
      {
        Type t = GetType();

        PropertyInfo ProInfo = t.GetProperty(member.Key);

        YZXAddress newAddress = AddressBase.Offset(member.Value);

        string datatype = ProInfo.PropertyType.Name;
        YZXDataTypes ytype = (YZXDataTypes)
          Enum.Parse(typeof(YZXDataTypes), datatype);

        switch (ytype)
        {
          case YZXDataTypes.YZXBit:
            break;
          case YZXDataTypes.YZXByte:
            break;
          case YZXDataTypes.YZXUshort:
            YZXUshort yushort = CPU.Mermories[newAddress.mIndex][newAddress.wIndex];
            ProInfo.SetValue(this, yushort, null);
            break;
          case YZXDataTypes.YZXUint:
            YZXUint yuint = CPU.Mermories[newAddress.mIndex].Uint[newAddress.wIndex];
            ProInfo.SetValue(this, yuint, null); 
            break;
        }
      }
    }

  }

  public enum YZXUDTTypes
  {
    CPUStatus = 1,
    UnitStatus = 2,
    TaskStatus =3,
    YZXCylinder = 4,
  }
}
