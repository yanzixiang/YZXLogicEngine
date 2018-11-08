using System;

using Eneter.Messaging.EndPoints.TypedMessages;
using Eneter.Messaging.MessagingSystems.MessagingSystemBase;

namespace YZXMessaging
{
  public class YZXResponseReceivedEventArgs<_ResponseMessageType> : EventArgs
  {
    public YZXResponseReceivedEventArgs(_ResponseMessageType responseMessage)
    {

    }
    public YZXResponseReceivedEventArgs(Exception error)
    {

    }

    public YZXResponseReceivedEventArgs(TypedRequestReceivedEventArgs<_ResponseMessageType> m){

    }

    public YZXResponseReceivedEventArgs(TypedResponseReceivedEventArgs<_ResponseMessageType> e)
    {
      ReceivingError = e.ReceivingError;
      ResponseMessage = e.ResponseMessage;
    }

    public Exception ReceivingError { get; private set; }
    public _ResponseMessageType ResponseMessage { get; private set; }
  }
  public sealed class YZXRequestReceivedEventArgs<_RequestMessageType> : EventArgs
  {
    public YZXRequestReceivedEventArgs(string responseReceiverId, 
      string senderAddress, 
      _RequestMessageType requestMessage)
    {

    }

    public YZXRequestReceivedEventArgs(string responseReceiverId,
      string senderAddress, Exception error)
    {

    }

    public YZXRequestReceivedEventArgs(TypedRequestReceivedEventArgs<_RequestMessageType> e)
    {
      ReceivingError = e.ReceivingError;
      RequestMessage = e.RequestMessage;
      ResponseReceiverId = e.ResponseReceiverId;
      SenderAddress = e.SenderAddress;
    }
    public Exception ReceivingError { get; private set; }

    public _RequestMessageType RequestMessage { get; private set; }
    public string ResponseReceiverId { get; private set; }
    public string SenderAddress { get; private set; }
  }

  public sealed class YZXResponseReceiverEventArgs : EventArgs
  {
    public string SenderAddress;
    public string ResponseReceiverId;
    public YZXResponseReceiverEventArgs(ResponseReceiverEventArgs e)
    {
      ResponseReceiverId = e.ResponseReceiverId;
      SenderAddress = e.SenderAddress;
    } 
  }

  public sealed class YZXDuplexChannelEventArgs : EventArgs
  {
    public string SenderAddress;
    public string ChannelId;
    public string ResponseReceiverId; 
    public YZXDuplexChannelEventArgs(DuplexChannelEventArgs e){
      SenderAddress = e.SenderAddress;
      ChannelId = e.ChannelId;
      ResponseReceiverId = e.ResponseReceiverId;
    }
  }
}
