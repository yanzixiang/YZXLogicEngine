using System;
using System.Collections.Generic;
using System.Net.Sockets;

using Modbus.Device;

using Extensions;

namespace YZXLogicEngine.Unit
{
  /// <summary>
  /// ModbusTCP模块
  /// 每次只能更新125个字
  /// </summary>
  public class RTAUnit : YZXWriteableUnit
  {
    public string IP{get; set;}
    public int PORT{get; set;}

    public ushort FromAddress { get; set; }

    public RTAUnit()
    {
      Init();
    }

    public override void Init()
    {
      bits = new List<bool>(Length);
    }

    #region YZXUnit
    public override List<bool> UpdateBits()
    {
      
      using (TcpClient client = new TcpClient(IP, PORT))
      {
        ModbusIpMaster modbusipmaster = ModbusIpMaster.CreateIp(client);
        ushort[]  uresults = modbusipmaster.ReadHoldingRegisters(FromAddress, Length);

        RecodeUpdatetimes();
        
        
        bits.Clear();
        for (int i = 0; i < Length; i++)
        {
          ushort randomint = uresults[i];
          bool[] rbits = randomint.ToBoolArray();
          for (int j = 0; j < 16; j++)
          {
            bits.Add(rbits[j]);
          }
        }
        return bits;
      }  
    }

    public override bool WriteB(ushort wIndex, byte bIndex, bool v)
    {
      throw new NotImplementedException();
    }

    public override bool WriteUshort(ushort wIndex, ushort value)
    {
      try
      {
        using (TcpClient client = new TcpClient(IP, PORT))
        {
          Writing = true;
          ModbusIpMaster modbusipmaster = ModbusIpMaster.CreateIp(client);
          modbusipmaster.WriteSingleRegister(wIndex, value);
          Writing = false;
        }
        return true;
      }
      catch (Exception ex)
      {
        Writing = false;
        Console.WriteLine(ex.ToString());
        return false;
      }
    }

    public override bool WriteD(ushort dwIndex, int d)
    {
      throw new NotImplementedException();
    }

    public override bool WriteBits(List<bool> bits)
    {
      throw new NotImplementedException();
    }
    #endregion YZXUnit
  }
}
