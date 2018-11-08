using System;

using Eneter.Messaging.EndPoints.TypedMessages;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;
using Eneter.Messaging.MessagingSystems.TcpMessagingSystem;
using Eneter.Messaging.MessagingSystems.HttpMessagingSystem;
using Eneter.Messaging.MessagingSystems.Composites;
using Eneter.Messaging.DataProcessing.Serializing;
using Eneter.Messaging.MessagingSystems.WebSocketMessagingSystem;

namespace YZXMessaging
{
  /// <summary>
  /// 消息客户端
  /// 主动向服务器发送请求
  /// 接收服务器返回的响应
  /// </summary>
  /// <typeparam name="RequestType">发出请求类型</typeparam>
  /// <typeparam name="ResponseType">期待响应类型</typeparam>
  public class YZXMessagingClient<RequestType, ResponseType>:YZXCMessagingNode
    where RequestType:YZXMessage,new()
    where ResponseType:YZXMessage,new()
  {
    public bool ReceiverConnectedToSender = false;

    #region 事件
    /// <summary>
    /// 连接关闭
    /// </summary>
    public event EventHandler<YZXDuplexChannelEventArgs> Sender_ConnectionClosedEvent;

    /// <summary>
    /// 连接打开
    /// </summary>
    public event EventHandler<YZXDuplexChannelEventArgs> Sender_ConnectionOpenedEvent;

    /// <summary>
    /// 请求接受到响应
    /// </summary>
    public event EventHandler<YZXResponseReceivedEventArgs<ResponseType>> Sender_ResponseReceivedEvent;
    #endregion 事件

    /// <summary>
    /// 请求发送
    /// </summary>
    public IDuplexTypedMessageSender<ResponseType, RequestType> DSender;

    public IDuplexOutputChannel anOutputChannel;

    public string Partner;
    public YZXMessagingClient(string partner, 
      YZXMessagesFactoryType messagetype = YZXMessagesFactoryType.Duplex)
    {
      Partner = partner;

      DSender = DSenderFactory.CreateDuplexTypedMessageSender<ResponseType, RequestType>();

      anOutputChannel = UnderlyingMessaging.CreateDuplexOutputChannel(Partner);



      ConfigSender();

      AttachOutputChannel();

    }

    public void AttachOutputChannel()
    {
      try
      {
        switch (MessagesFactoryType)
        {
          case YZXMessagesFactoryType.Duplex:
            if (DSender == null)
            {
              ConfigSender();
            }
            else
            {
              if (DSender.IsDuplexOutputChannelAttached)
              {

              }
              else
              {
                try
                {
                  anOutputChannel = UnderlyingMessaging.CreateDuplexOutputChannel(Partner);
                  DSender.AttachDuplexOutputChannel(anOutputChannel);
                }
                catch (Exception ex)
                {
                  Console.WriteLine(ex);
                }

              }
            }
            break;
        }
      }
      catch (Exception EX)
      {
        Console.WriteLine(EX);
      }
    }

    public void ConfigSender()
    {
      switch (MessagesFactoryType)
      {
        case YZXMessagesFactoryType.Duplex:
          ConfigDuplexSender();
          break;
      }
    }

    public void ConfigDuplexSender() {
      DSender.ConnectionClosed += Sender_ConnectionClosed;
      DSender.ConnectionOpened += Sender_ConnectionOpened;
      DSender.ResponseReceived += Sender_ResponseReceived;
    }

    public override void StopMessaging()
    {
      if (DSender.IsDuplexOutputChannelAttached)
      {
        DSender.DetachDuplexOutputChannel();
      }
    }

    #region Sender 事件
    void Sender_ConnectionClosed(object sender, DuplexChannelEventArgs e)
    {
      if (Sender_ConnectionClosedEvent != null)
      {
        YZXDuplexChannelEventArgs ye = new YZXDuplexChannelEventArgs(e);
        Sender_ConnectionClosedEvent(sender, ye);
      }
    }

    void Sender_ConnectionOpened(object sender, DuplexChannelEventArgs e)
    {
      if (Sender_ConnectionOpenedEvent != null)
      {
        YZXDuplexChannelEventArgs ye = new YZXDuplexChannelEventArgs(e);
        Sender_ConnectionOpenedEvent(sender, ye);
      }
    }

    public void Sender_ResponseReceived(object sender, TypedResponseReceivedEventArgs<ResponseType> e)
    {
      YZXResponseReceivedEventArgs<ResponseType> ye = new YZXResponseReceivedEventArgs<ResponseType>(e);
      Sender_ResponseReceivedEvent(sender, ye);
    }
    #endregion Sender 事件
    public bool IsAttached
    {
      get
      {
        switch (MessagesFactoryType)
        {
          case YZXMessagesFactoryType.Duplex:
            return DSender.IsDuplexOutputChannelAttached;
          default:
            return false;
        }
      }
    }

    public string SendRequest(RequestType request)
    {
      string result = "";
      switch (MessagesFactoryType)
      {
        case YZXMessagesFactoryType.Duplex:
          DSender.SendRequestMessage(request);
          break;
      }
      return result;
    }
  }
}