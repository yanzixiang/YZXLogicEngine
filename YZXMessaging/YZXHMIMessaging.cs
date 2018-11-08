namespace YZXMessaging
{
  /// <summary>
  /// HMI客户端
  /// 想要向HMI发送请求，比如CPU，使用
  /// 主动发送YZXHMIRequest,等待YZXHMIResponse
  /// </summary>
  /// <param name="PartnerCPU"></param>
  public class YZXHMIClient :
      YZXMessagingClient<YZXHMIRequest, YZXHMIResponse>
  {
    public YZXHMIClient(string PartnerCPU)
      : base(PartnerCPU)
    {
    }
  }

  /// <summary>
  /// HMI服务器
  /// 被动接收YZXHMIRequest,返回YZXHMIResponse
  /// </summary>
  /// <param name="HMI">HMI连接字符串</param
  public class YZXHMIServer :
      YZXMessagingServer<YZXHMIRequest, YZXHMIResponse>
  {
    public YZXHMIServer(string SelfHMI)
      : base(SelfHMI)
    {
    }
  }
}
