using System;
using System.IO.Ports;

namespace YZXLogicEngine.Unit
{
  /// <summary>
  /// Balluff BIS M-4 RFID 读写头
  /// 通过串口控制
  /// </summary>
  public class BISM4
  {
    private SerialPort SP;

    public BISM4(SerialPort sp)
    {
      SP = sp;
      //设置每行结尾为回车符 CR 13(Decimal) OD(HEX)
      SP.NewLine = "\r";
    }

    public static readonly string STX = "\u0002";
    public static readonly string ACK0 = "\u00060";
    public static readonly string LPEnd = "11";

    ~BISM4()
    {
      try
      {
        SP.Close();
      }catch(Exception ex)
      {
        Console.WriteLine("~BISM4" + ex.ToString());
      }
    }

    private object sendLock = new object();
    /// <summary>
    /// 发送
    /// </summary>
    /// <param name="s"></param>
    public void Send(string s)
    {
      try
      {
        lock (sendLock)
        {
          if (!SP.IsOpen)
          {
            SP.Open();
          }
          SP.WriteLine(s);
        }
      }catch(Exception ex)
      {
        Console.WriteLine("Send" + ex.ToString());
      }
    }

    public string Read(int count = 0)
    {
      lock (sendLock)
      {
        string s = "";
        while(s == "")
        {
          if (count == 0)
          {
            s = SP.ReadLine();
          }
          else
          {
            char[] ca = new char[count];
            int rc = SP.Read(ca, 0, count);
            s = new string(ca);
          }
        }
        return s;
      }
    }

    /// <summary>
    /// 读取存储器序列号
    /// </summary>
    public string U()
    {
      string s = "U";
      Send(s);
      string ur = Read();
      return ur;
    }

    /// <summary>
    /// P
    /// 写入数据
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="count">字节数</param>
    /// <param name="value">数据</param>
    public void P(int address,int count,string value)
    {
      string s = "P" + address.ToString("D4") + count.ToString("D4") + LPEnd;
      string STXString = STX + value;
      Send(s);
      string lr = Read();
      if(lr == ACK0)
      {
        Send(STXString);
      }
      else
      {

      }
      lr = Read();
    }

    /// <summary>
    /// L 
    /// 读取数据
    /// </summary>
    /// <param name="address">地址</param>
    /// <param name="count">字节数</param>
    /// <returns>数据</returns>
    public string L(int address,int count)
    {
      if(count <= 0) {
        return "";
      }
      string s = "L" + address.ToString("D4") + count.ToString("D4") + LPEnd;
      string STXString = STX;
      Send(s);
      string lr = Read();
      if(lr == ACK0)
      {
        Send(STXString);
        string lr2 = Read(0);
        return lr2;
      }
      else
      {
        return "Error";
      }
    }
  }
}
