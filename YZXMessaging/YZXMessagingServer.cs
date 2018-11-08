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
  /// 消息服务器
  /// 处理来自消息客户端的请求
  /// 不主动发送消息
  /// </summary>
  /// <typeparam name="RequestType">发出请求类型</typeparam>
  /// <typeparam name="ResponseType">期待响应类型</typeparam>
  public class YZXMessagingServer<RequestType, ResponseType>:YZXCMessagingNode
    where RequestType : YZXMessage, new()
    where ResponseType : YZXMessage, new()
  {
    public bool ReceiverConnectedToSender = false;

    #region 事件
    /// <summary>
    /// 接收到请求 
    /// </summary>
    public event EventHandler<YZXRequestReceivedEventArgs<RequestType>> Receiver_MessageReceivedEvent;

    /// <summary>
    /// 客户端连接成功
    /// </summary>
    public event EventHandler<YZXResponseReceiverEventArgs> Receiver_ResponseReceiverConnectedEvent;

    /// <summary>
    /// 客户端断开连接
    /// </summary>
    public event EventHandler<YZXResponseReceiverEventArgs> Receiver_ResponseReceiverDisconnectedEvent;

    #endregion 事件

    /// <summary>
    /// 响应接受
    /// </summary>
    public IDuplexTypedMessageReceiver<ResponseType, RequestType> DReceiver;

    public IDuplexInputChannel anInputChannel;

    public string Self;
    public YZXMessagingServer(string self,
      YZXMessagesFactoryType messagetype = YZXMessagesFactoryType.Duplex)
    {
      Self = self;

      ConfigReceiver();

      AttachInputChannel();
    }

    public void AttachInputChannel()
    {
      anInputChannel = UnderlyingMessaging.CreateDuplexInputChannel(Self);

      switch (MessagesFactoryType)
      {
        case YZXMessagesFactoryType.Duplex:
          DReceiver.AttachDuplexInputChannel(anInputChannel);
          break;
      }
    }

    public void ConfigReceiver()
    {
      switch (MessagesFactoryType)
      {
        case YZXMessagesFactoryType.Duplex:
          ConfigDuplexReceiver();
          break;
      }
    }

    public void ConfigDuplexReceiver()
    {
      DReceiver = DReceiverFactory.CreateDuplexTypedMessageReceiver<ResponseType, RequestType>();
      DReceiver.MessageReceived += Receiver_MessageReceived;
      DReceiver.ResponseReceiverConnected += Receiver_ResponseReceiverConnected;
      DReceiver.ResponseReceiverDisconnected += Receiver_ResponseReceiverDisconnected;
    }

    public override void StopMessaging()
    {
      if (DReceiver != null & DReceiver.IsDuplexInputChannelAttached)
      {
        DReceiver.DetachDuplexInputChannel();
      }
    }


    #region Receiver 事件
    public void Receiver_MessageReceived(object sender, TypedRequestReceivedEventArgs<RequestType> e)
    {
      try
      {
        RequestType request = e.RequestMessage;
        request.SenderAddress = e.SenderAddress;
        request.ResponseReceiverId = e.ResponseReceiverId;

        if (Receiver_MessageReceivedEvent != null)
        {
          Receiver_MessageReceivedEvent(sender, new YZXRequestReceivedEventArgs<RequestType>(e));
        }
      }
      catch (Exception ex)
      {
      }
    }

    void Receiver_ResponseReceiverDisconnected(object sender, ResponseReceiverEventArgs e)
    {
      if (Receiver_ResponseReceiverDisconnectedEvent != null)
      {
        YZXResponseReceiverEventArgs ye = new YZXResponseReceiverEventArgs(e);
        Receiver_ResponseReceiverDisconnectedEvent(sender, ye);
      }
    }

    void Receiver_ResponseReceiverConnected(object sender, ResponseReceiverEventArgs e)
    {
      if (Receiver_ResponseReceiverConnectedEvent != null)
      {
        YZXResponseReceiverEventArgs ye = new YZXResponseReceiverEventArgs(e);
        Receiver_ResponseReceiverConnectedEvent(sender, ye);
      }
    }

    #endregion Receiver 事件
    public void SendResponse(string responseReceiverId,ResponseType response)
    {
      try
      {
        switch (MessagesFactoryType)
        {
          case YZXMessagesFactoryType.Duplex:
            if (DReceiver.IsDuplexInputChannelAttached)
            {
              DReceiver.SendResponseMessage(responseReceiverId, response);
            }
            else
            {

            }
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(responseReceiverId);
        Console.WriteLine(ex.ToString());
      }
    }
  }
}