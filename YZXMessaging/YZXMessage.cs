using System;

#if DOT42
#else
using System.ComponentModel;
#endif

namespace YZXMessaging
{
  /// <summary>
  /// 消息种类
  /// </summary>
  public enum YZXMessageType
  {
    Request = 0,
    Response = 1
  }
  
  /// <summary>
  /// 消息
  /// </summary>
  public class YZXMessage
  {
    public string SenderAddress;
    public string ResponseReceiverId;
    //public Exception ReceivingError;

    public int Type;
    public int OPCode;
    public string OPAddress;
    public string[] OPString = new String[10];
    public byte[] OPValue = new byte[20];
    //public Dictionary<string,object> Params;

    public YZXMessage()
    {
      //Params = new Dictionary<string,object>();
    }

    public YZXMessageSort GetSort() { 
      YZXMessageSort sort = new YZXMessageSort();
      sort.OPCode = OPCode;
      sort.OPAddress = OPAddress;
      return sort;
    }
  }

  /// <summary>
  /// 用于消息分类
  /// </summary>
  public class YZXMessageSort
  {
    public YZXMessageSort()
    {
      index = "local";
      OPCode = (int)YZXCPUOPType.ReadUshort;
      OPAddress = "R.0";
    }

    /// <summary>
    /// 操作码
    /// CPU向HMI发送请求种类
    /// </summary>
    #if (!DOT42)
    [Category("远程")]
    [DisplayName("CPU索引")]
    [Description("This property uses a TextBox as the default editor.")]
    #endif
    public string index{get;set;}

    /// <summary>
    /// 操作码
    /// CPU向HMI发送请求种类
    /// </summary>
    #if (!DOT42)
    [Category("远程")]
    [DisplayName("操作码")]
    [Description("This property uses a TextBox as the default editor.")]
    #endif
    public int OPCode{get;set;}

    /// <summary>
    /// 操作地址
    /// </summary>
    #if (!DOT42)
    [Category("远程")]
    [DisplayName("操作地址")]
    [Description("This property uses a TextBox as the default editor.")]
    #endif
    public string OPAddress{get;set;} 

    public override string ToString()
    {
      string s = String.Format("{0}-{1}-{2}", index, OPCode, OPAddress);
      return s;
    }

    #region 类型转换
    //public static implicit operator EventArgs(YZXMessage m)
    //{
    //  EventArgs eargs = new EventArgs();
    //  return eargs;
    //}
    #endregion 类型转换
  }
}
