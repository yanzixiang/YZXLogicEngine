using System;
using System.Collections.Generic;

using Common.Threading;

using Extensions;

namespace YZXLogicEngine.Unit
{
  /// <summary>
  /// Snap7Client连接的一个数据块
  /// </summary>
  public class Snap7ClientDBUnit : YZXWriteableUnit
  {
    public YZXS7Client Client { get;  set; }
    public int DBNum { get; set; }

    public Snap7ClientDBUnit()
    {
      Init();
    }

    public override void Init(){
      bits = new List<bool>(Length);
      UpdateLock = new OrderedLock();

      Buffer = new byte[Length];
      bits = new List<bool>(Length * 16);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="DBnum"></param>
    /// <param name="Length"></param>
    public Snap7ClientDBUnit(YZXS7Client client, int DBnum, ushort length = 512)
    {
      
      Client = client;
      Name = string.Format("{0}-{1}-{2}", client.Name, "DB", DBNum);

      DBNum = DBnum;
      Length = length;

      Init();
    }

    private byte[] Buffer; 

    #region YZXUnit
    public override List<bool> UpdateBits()
    {
      bool success = false;
      try
      {
        RecodeUpdatetimes();

        int res = -1;
        res = Client.DBRead(DBNum, 0, Length, Buffer);
        if (res == 0)
        {
          success = true;
          RecodeSuccessTimes();
        }
        else
        {
          RecodeErrorTimes();
        }
      }
      catch (Exception ex)
      {
        RecodeErrorTimes(ex);
      }

      if (success)
      {
        bits.Clear();
        for (int i = 0; i < Length; i++)
        {
          short randomint = (short)Buffer[i];
          bool[] rbits = randomint.ToBoolArray();
          for (int j = 0; j < 16; j++)
          {
            bits.Add(rbits[j]);
          }
        }
      }
      return bits;
    }

    public override bool WriteB(UInt16 wIndex,byte bIndex,bool v)
    {
      throw new NotImplementedException();
    }

    public override bool WriteUshort(UInt16 wIndex,UInt16 i)
    {
      byte[] bytes = BitConverter.GetBytes(i);
      int res = Client.DBWrite(DBNum, wIndex, 2, bytes);

      byte[] resbytes = BitConverter.GetBytes(res);
      //Console.WriteLine(resbytes.HexDump(20, 16));

      if (res == 0)
      {
        return true;
      }
      else
      {
        return false;
      }
    }

    public override bool WriteD(UInt16 dwIndex,Int32 d)
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
