namespace YZXMessaging
{
  /// <summary>
  /// CPU接受的请求
  /// </summary>
  public class YZXCPURequest : YZXMessage
  {
    public YZXCPURequest()
    {
      Type = (int)YZXMessageType.Request;
    }

    public YZXCPURequest(YZXCPUOPType opcode)
      : base()
    {
      Type = (int)YZXMessageType.Request;
      OPCode = (int)opcode;
    }

    public override string ToString()
    {
      string s = string.Format("{0} - {1} - {2}",SenderAddress,OPCode,OPValue);
      return s;
    }
  }

  /// <summary>
  /// CPU返回的响应
  /// </summary>
  public class YZXCPUResponse : YZXMessage
  {
    public YZXCPUResponse()
    {
      Type = (int)YZXMessageType.Response;
    }
  }

  /// <summary>
  /// CPU消息种类
  /// </summary>
  public enum YZXCPUOPType
  {
    ReadBit = 0,              //读位
    WriteBit = 1,             //写位
    SetBit = 2,               //置位
    ResetBit = 3,             //复位
    RevertBit = 4,            //反转位
    ReadUshort = 5,           //读无符号字
    WriteUshort = 6,          //写无符号字
    BroadcastMessage = 7,     //广播消息
    SendMessage = 8,          //向指定地址发送消息
    ReadUint = 9,             //读无符号双字
    WriteUint = 10,           //写无符号双字
    GetAvailableMemery = 11,  //获取可用内存区域
    GetAvailableUnit = 12,    //获取可用模块
  }
}
