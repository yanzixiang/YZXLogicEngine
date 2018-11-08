using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading.Tasks;

using Modbus.Device;

using Extensions;


namespace YZXLogicEngine.Unit
{
  /// <summary>
  /// ModbusTCP模块
  /// 每次只能更新125个字
  /// </summary>
  public class ModbusRTUUnit : YZXWriteableUnit
  {
    #region 串口相关
    private SerialPort port; //使用的端口
    public byte ComNum{get; private set;}
    public int baudrate{get;private set;}
    public int databits{get;private set;}
    public Parity parity{get;private set;}
    public StopBits stopbits{get;private set;}
    #endregion 串口相关
    public byte PLCaddress {get;private set;}

    
    private IModbusSerialMaster master; //使用的控制器

    public ushort FromAddress { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="comn"></param>
    /// <param name="bautrate"></param>
    /// <param name="parity"></param>
    /// <param name="databits"></param>
    /// <param name="stopbits"></param>
    /// <param name="address"></param>
    public ModbusRTUUnit(byte comn,
      int bautrate,
      Parity parity,
      int databits,
      StopBits stopbits,
      byte address)
    {
      ComNum = comn;
			this.baudrate = bautrate;
			this.parity = parity;
			this.databits = databits;
			this.stopbits = stopbits;
			this.PLCaddress = address;

      string com = "COM" +　comn.ToString();
      port = new SerialPort(com, baudrate, parity, databits, stopbits);
      port.Open();
      master = ModbusSerialMaster.CreateRtu(port);

    }

    #region YZXUnit
    public override List<bool> UpdateBits()
    {
      
      //ModbusIpMaster modbusipmaster = GetIpMaster();
      ushort[] uresults = modbusipmaster.ReadHoldingRegisters(FromAddress, Length);
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

    public override void WriteB(int wIndex, int bIndex, bool v)
    {
    }

    public override void WriteW(int wIndex, short i)
    {
    }

    public override void WriteD(int dwIndex, int d)
    {
    }
    #endregion YZXUnit
  

  }
}
