using System;
using System.Collections.Generic;
using System.Collections.Specialized;

using Common;
using Extensions;

namespace YZXLogicEngine.Unit
{
  /// <summary>
  /// UR20WEB模块
  /// </summary>
  public class UR20WEBUnit : YZXWriteableUnit
  {
    public string IP{get; set;}
    public string TO{get; set;}

    public ushort FromAddress { get; set; }

    public UR20WEBUnit(string ip = "192.168.2.203",
      string to = "d41d8cd98f00b204e9800998ecf8427e",
      ushort from = 0, ushort count =100):
      base(ip,count)
    {
      
      Name = string.Format("UR20WEB-{0}-{1}", ip ,to);
      IP = ip;
      TO = to;
      FromAddress = from;
      Length = count;

      Init();
    }
    public override void Init()
    {
      bits = new List<bool>(Length);

    }

    public bool ActiveForcen()
    {
      string url = string.Format("http://{0}/mock/write.php", IP);
      try
      {
        NameValueCollection nvc = new NameValueCollection();
        nvc.Add("to", TO);
        nvc.Add("fr[0][cn]", "fm");
        Http.Post(url, nvc);
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
    }
    public bool DeactiveForcen()
    {
      string url = string.Format("http://{0}/mock/write.php", IP);
      try
      {
        NameValueCollection nvc = new NameValueCollection();
        nvc.Add("to", TO);
        nvc.Add("fr[0][cn]", "uc");
        Http.Post(url, nvc);
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
    }

    #region YZXUnit
    public override List<bool> UpdateBits()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wIndex"></param>
    /// <param name="bIndex"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    public override bool WriteB(ushort wIndex, byte bIndex, bool v)
    {
      string url = string.Format("http://{0}/mock/write.php", IP);
      try
      {
        NameValueCollection nvc = new NameValueCollection();
        nvc.Add("to", TO);
        nvc.Add("fr[0][cn]", "fc");
        string bIndexString = bIndex.ToString().PadLeft(2, '0');
        string wIndexString = wIndex.ToString().PadLeft(2, '0');
        string keystring = string.Format("{0}{1}{2}", wIndexString, bIndexString, v ? 1 : 0);
        nvc.Add("fr[0][df]", keystring);
        Http.Post(url,nvc);
        return true;
      }catch(Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
    }

    public bool UpdateUshort(ushort wIndex, short oldValue, short newValue)
    {
      return UpdateUshort(wIndex, (ushort)oldValue, (ushort)newValue);
    }
    /// <summary>
    /// 更新值
    /// 为减少POST请求数量
    /// </summary>
    /// <param name="wIndex">字地址</param>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    /// <returns></returns>
    public bool UpdateUshort(ushort wIndex, ushort oldValue, ushort newValue)
    {
      string url = string.Format("http://{0}/mock/write.php", IP);
      try
      {
        NameValueCollection nvc = new NameValueCollection();
        nvc.Add("to", TO);
        bool[] oldBits = oldValue.ToBoolArray();
        bool[] newBits = newValue.ToBoolArray();
        string wIndexString = wIndex.ToString().PadLeft(2, '0');
        for (byte bIndex = 0; bIndex <= 15; bIndex++)
        {
          bool oldV = oldBits[bIndex];
          bool newV = newBits[bIndex];

          if(oldV != newV)
          {
            WriteB(wIndex, bIndex, newV);
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return false;
      }
    }

    public override bool WriteUshort(ushort wIndex, ushort value)
    {
      string url = string.Format("http://{0}/mock/write.php", IP);
      try
      {
        NameValueCollection nvc = new NameValueCollection();
        nvc.Add("to", TO);
        bool[] bits =  value.ToBoolArray();
        string wIndexString = wIndex.ToString().PadLeft(2, '0');
        for (byte bIndex = 0; bIndex <= 15; bIndex++)
        {
          bool v = bits[bIndex];
          //string bIndexString = bIndex.ToString().PadLeft(2, '0');

          //nvc.Add(String.Format("fr[{0}][cn]", bIndexString), "fc");
          //string keystring = String.Format("{0}{1}{2}", wIndexString, bIndex, v ? 1 : 0);
          //nvc.Add(String.Format("fr[{0}][df]", bIndexString), keystring);
          WriteB(wIndex, bIndex, v);
        }
        //Http.Post(url, nvc);
        return true;
      }
      catch (Exception ex)
      {
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
