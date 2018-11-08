using System;
using System.Net;
using System.Diagnostics;
using System.Net.NetworkInformation;

using Eneter.Messaging.EndPoints.TypedMessages;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;
using Eneter.Messaging.MessagingSystems.TcpMessagingSystem;
using Eneter.Messaging.MessagingSystems.HttpMessagingSystem;
using Eneter.Messaging.MessagingSystems.WebSocketMessagingSystem;
using Eneter.Messaging.DataProcessing.Serializing;

namespace YZXMessaging
{
  public abstract class YZXCMessagingNode
  {
    public IDuplexTypedMessagesFactory DSenderFactory;

    public IDuplexTypedMessagesFactory DReceiverFactory;

    public IMessagingSystemFactory UnderlyingMessaging;

    public ISerializer serializer;

    public YZXMessagesFactoryType MessagesFactoryType;

    public YZXCMessagingNode(
      YZXMessagingType messagingtype = YZXMessagingType.Tcp,
      YZXSerializerType serializertype = YZXSerializerType.Xml,
      YZXMessagesFactoryType factorytype = YZXMessagesFactoryType.Duplex)
    {
      DSenderFactory = new DuplexTypedMessagesFactory();

      DReceiverFactory = new DuplexTypedMessagesFactory();

      switch (messagingtype)
      {
        case YZXMessagingType.Tcp:
          UnderlyingMessaging = new TcpMessagingSystemFactory();
          break;
        case YZXMessagingType.Http:
          UnderlyingMessaging = new HttpMessagingSystemFactory();
          break;
        case YZXMessagingType.WebSocket:
          UnderlyingMessaging = new WebSocketMessagingSystemFactory();
          break;
      }

      switch(serializertype){
        case YZXSerializerType.Xml:
          serializer = new XmlStringSerializer();
          break;
        case YZXSerializerType.Json:
          serializer = new DataContractJsonStringSerializer();
          break;
      }
    }

    public static byte[] ServerIPRange = new byte[3] { 192, 168, 1 };
    public static int ServerMessagingPort = 8034;
    public static int ClientMessagingPort = 8035;
    public static string ResolveServerIP()
    {
      String hostname = Dns.GetHostName();
      IPAddress[] addresses = Dns.GetHostAddresses(hostname);
      foreach (IPAddress ip in addresses)
      {
        byte[] bytes = ip.GetAddressBytes();
        if (bytes.Length == 4)
        {
          //只处理IPv4地址
          if (bytes[0] == ServerIPRange[0]
            & bytes[1] == ServerIPRange[1]
            & bytes[2] == ServerIPRange[2])
          {
            string ServerIP = string.Format("{0}.{1}.{2}.{3}",
              bytes[0], bytes[1], bytes[2], bytes[3]);
            return ServerIP;
          }
        }       
      }
      return "";
    }

    public static int maxWorkingSet = 1024000;
    public static void SetWorkingSet(int maxWorkingSet = 1024000)
    {
      IntPtr min = Process.GetCurrentProcess().MinWorkingSet;
      try
      {
        Process.GetCurrentProcess().MaxWorkingSet = (IntPtr)maxWorkingSet;
      }
      catch (Exception ex)
      {
        Process.GetCurrentProcess().MaxWorkingSet = min;
      }

    }

    public abstract void StopMessaging();

    public bool online = false;
    private void SetupNetworkChange()
    {
      if (NetworkInterface.GetIsNetworkAvailable())
      {
        online = true;
      }
      else
      {
        online = false;
      }

      NetworkChange.NetworkAddressChanged +=
          new NetworkAddressChangedEventHandler(OnNetworkChange);
    }

    public void OnNetworkChange(object sender, EventArgs e)
    {
      if (NetworkInterface.GetIsNetworkAvailable())
      {
        if (!online)
        {
          online = true;
        }
      }
      else
      {
        if (online)
        {
          online = false;
          StopMessaging();
        }
      }
    }
  }

  public enum YZXMessagingType
  {
    Tcp = 0,
    Http = 1,
    WebSocket = 2
  }

  public enum YZXSerializerType
  {
    Xml = 0,
    Json = 1
  }

  public enum YZXMessagesFactoryType
  {
    Duplex = 0,
    Reliable = 1
  }
}
