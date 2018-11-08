using System;
using System.Collections.Generic;

using Common;
using Extensions;

using YZXMessaging;

namespace YZXLogicEngine
{
  /// <summary>
  /// CPU
  /// 针对设备编程
  /// </summary>
  public partial class YZXCPU
  {
    public Dictionary<string, YZXHMIClient> HMIs;

    #region 读内存区域
    public YZXUint GetUint(string address)
    {
      string[] ss = address.Split(YZXAddress.AddressDelimiterChars);
      if (ss.Length != 3)
      {
      }
      string mIndex = ss[0];
      short wIndex = ss[1].ToShort();

      return Mermories[mIndex].Uint[wIndex];
    }

    public YZXUshort GetUshort(string address)
    {
      string[] ss = address.Split(YZXAddress.AddressDelimiterChars);
      if (ss.Length != 3)
      {
      }
      string mIndex = ss[0];
      short wIndex = ss[1].ToShort();

      return Mermories[mIndex][wIndex];
    }
    public YZXBit GetBit(string address)
    {
      string[] ss = address.Split(YZXAddress.AddressDelimiterChars);
      if (ss.Length != 3)
      {
      }
      string mIndex = ss[0];
      short wIndex = ss[1].ToShort();
      short bIndex = ss[2].ToShort();

      return Mermories[mIndex][wIndex][bIndex];
    }
    #endregion 读内存区域

    public void StopMessaging()
    {
      foreach (YZXHMIClient hmi in HMIs.Values)
      {
        hmi.StopMessaging();
      }

      Server.StopMessaging();
    }

    #region 通讯
    

    public string ServerIP = "";
    public void StartCPUServer()
    {
      
      if (ServerIP != "")
      {
        string ServerAddress =
          string.Format("tcp://{0}:{1}", ServerIP,
          YZXCMessagingNode.ServerMessagingPort);
        Server = new YZXCPUServer(ServerAddress);
        Server.Receiver_MessageReceivedEvent += YZXCPU_Receiver_MessageReceivedEvent;
        Console.WriteLine("Server Started at {0}", ServerAddress);
      }
      else
      {
        throw new InvalidOperationException();
      }
    }


    public void StartHttpServer()
    {
    }


    public void DisconnectFromHMI(string HMIConnectString)
    {
      try
      {
        HMIs.Remove(HMIConnectString);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
    }

    /// <summary>
    /// 连接到HMI
    /// </summary>
    public void ConnectToHMI(string HMIConnectString,bool overwrite=false)
    {
      string HMIServerConnectString = string.Format("tcp://{0}:{1}", 
        HMIConnectString, YZXCMessagingNode.ClientMessagingPort);
      if (overwrite)
      {
        HMIs[HMIConnectString] = new YZXHMIClient(HMIServerConnectString);
        Console.WriteLine("OverWriten");
      }
      else
      {
        if (HMIs.ContainsKey(HMIConnectString))
        {
          Console.WriteLine("Already Connected");
        }
        else
        {
          HMIs[HMIConnectString] = new YZXHMIClient(HMIServerConnectString);
          
        }
      }

      
    }

    void YZXCPU_Receiver_MessageReceivedEvent(object sender, 
      YZXRequestReceivedEventArgs<YZXCPURequest> e)
    {
      YZXCPURequest req = e.RequestMessage;
      string id = e.ResponseReceiverId;
      YZXCPUResponse response = ParseHMIRequest(req);
      Server.SendResponse(id,response);
    }

    /// <summary>
    /// 解析接受到的请求
    /// </summary>
    /// <param name="CPURequest">CPU请求</param>
    /// <returns>CPU响应</returns>
    public YZXCPUResponse ParseHMIRequest(YZXCPURequest CPURequest)
    {
      string id = CPURequest.ResponseReceiverId;

      YZXCPUResponse response = new YZXCPUResponse();

      response.OPCode = CPURequest.OPCode;

      string address = CPURequest.OPAddress;

      response.OPAddress = CPURequest.OPAddress; 
      

      switch (CPURequest.OPCode)
      {
        case (int)YZXCPUOPType.ReadUshort:
          return ReadUshort(CPURequest);
        case (int)YZXCPUOPType.WriteUshort:
          break;
        case (int)YZXCPUOPType.ReadBit:
          break;
        case (int)YZXCPUOPType.WriteBit:
          break;
        case (int)YZXCPUOPType.SetBit:
          break;
        case (int)YZXCPUOPType.ResetBit:
          break;
        case (int)YZXCPUOPType.ReadUint:
          return ReadUint(CPURequest);
        case (int)YZXCPUOPType.WriteUint:
          break;
        case (int)YZXCPUOPType.GetAvailableMemery:
          return GetAvailableMemery(CPURequest);
        case (int)YZXCPUOPType.GetAvailableUnit:
          return GetAvailableUnit(CPURequest);
      }

      return response;
    }

    private YZXCPUResponse ReadUint(YZXCPURequest CPURequest)
    {
      string id = CPURequest.ResponseReceiverId;

      YZXCPUResponse response = new YZXCPUResponse();

      response.OPCode = CPURequest.OPCode;

      string address = CPURequest.OPAddress;

      response.OPAddress = CPURequest.OPAddress; 

      uint u = (uint)GetUint(address);
      response.OPValue = BitConverter.GetBytes(u);
      Console.WriteLine(string.Format("响应了请求{0} -> {1} -> {2} {3}",
        response.OPAddress,
        u,
        response.OPValue[0],
        response.OPValue[1]));
      return response;
    }

    private YZXCPUResponse ReadUshort(YZXCPURequest CPURequest)
    {
      string id = CPURequest.ResponseReceiverId;

      YZXCPUResponse response = new YZXCPUResponse();

      response.OPCode = CPURequest.OPCode;

      string address = CPURequest.OPAddress;

      response.OPAddress = CPURequest.OPAddress; 

      int i = GetUshort(address);
      response.OPValue = BitConverter.GetBytes(i);

      Console.WriteLine(string.Format("响应了请求{0} -> {1} -> {2} {3}",
        response.OPAddress,
        i,
        response.OPValue[0],
        response.OPValue[1]));

      return response;
    }
    private YZXCPUResponse GetAvailableMemery(YZXCPURequest CPURequest)
    {
      string id = CPURequest.ResponseReceiverId;

      YZXCPUResponse response = new YZXCPUResponse();

      response.OPCode = CPURequest.OPCode;
      response.OPAddress = CPURequest.OPAddress;

      List<string> Names = new List<string>();
      List<int> Lengths = new List<int>();

      foreach (YZXCPUMemory memory in Mermories.Values)
      {
        Names.Add(memory.Name);
        Lengths.Add(memory.Length);
      }

      response.OPString[0] = string.Join(",", Names) ;
      response.OPString[1] = string.Join(",",Lengths);

      return response;
    }

    private YZXCPUResponse GetAvailableUnit(YZXCPURequest CPURequest)
    {
      string id = CPURequest.ResponseReceiverId;

      YZXCPUResponse response = new YZXCPUResponse();

      response.OPCode = CPURequest.OPCode;
      response.OPAddress = CPURequest.OPAddress;

      List<string> Names = new List<string>();

      foreach (YZXUnit memory in Units.Values)
      {
        Names.Add(memory.Name);
      }

      response.OPString[0] = string.Join(",", Names);

      return response;
    }
    #endregion 通讯
  }
}