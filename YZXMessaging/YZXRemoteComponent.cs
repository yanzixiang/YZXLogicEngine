namespace YZXMessaging
{

  /// <summary>
  /// 远程控件
  /// </summary>
  /// <typeparam name="RequestType">发送请求的类型</typeparam>
  /// <typeparam name="ResponseType">接收响应的类型</typeparam>
  public interface YZXRemoteControl<RequestType, ResponseType>
  {
    RequestType Request();
    void ParseResponse(ResponseType response);


  }
}
