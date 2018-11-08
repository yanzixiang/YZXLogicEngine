namespace YZXMessaging
{
  /// <summary>
  /// HMI接受的请求
  /// </summary>
  public class YZXHMIRequest : YZXMessage
  {
    public YZXHMIRequest()
    {
      Type = (int)YZXMessageType.Request;
    }

   public YZXHMIRequest(YZXHMIOPType opcode)
      : base()
    {
      Type = (int)YZXMessageType.Request;
      OPCode = (int)opcode;
    }
  }

  /// <summary>
  /// HMI返回的响应
  /// </summary>
  public class YZXHMIResponse : YZXMessage
  {
    public YZXHMIResponse()
    {
      Type = (int)YZXMessageType.Response;
    }
  }

  /// <summary>
  /// HMI消息种类
  /// </summary>
  public enum YZXHMIOPType
  {
    GetCurrentPage = 0,             //获取当前页面号
    SetCurrentPage = 1,             //设置页面
    SendNotification = 2,           //发送通知
    SendMessage = 3,                //发送消息
  }
}
