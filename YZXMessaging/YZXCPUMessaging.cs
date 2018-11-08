namespace YZXMessaging
{
  /// <summary>
  /// CPU客户端
  /// 想要向CPU发送请求，比如HMI，使用
  /// 主动发送YZXCPURequest,等待YZXCPUResponse
  /// </summary>
  /// <param name="PartnerCPU">CPU连接字符串</param>
  public class YZXCPUClient :
      YZXMessagingClient<YZXCPURequest, YZXCPUResponse>
  {
    public YZXCPUClient(string PartnerHMI)
      : base(PartnerHMI)
    {
    } 
  }

  /// <summary>
  /// CPU服务器
  /// 被动接收YZXCPURequest,返回YZXCPUResponse
  /// </summary>
  /// <param name="SelfCPU">HMI连接字符串</param>
  public class YZXCPUServer :
      YZXMessagingServer<YZXCPURequest, YZXCPUResponse>
  {
    public YZXCPUServer(string SelfCPU)
      : base(SelfCPU)
    {
    }
  }
}
