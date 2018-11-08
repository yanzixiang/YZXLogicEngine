using System;

using Sharp7;

namespace YZXLogicEngine.Unit
{
  /// <summary>
  /// 
  /// </summary>
  public class YZXS7Client:S7Client
  {
    public string Name { get; set; }
    public string IP { get; set; }
    public int Rack { get; set; }
    public int Slot { get; set; }

    public YZXS7Client()
    {
      Init();
    }

    public void Init()
    {

    }

    public YZXS7Client(string name,string ip,int rack =0 ,int slot=2):
      base()
    {
      Init();
      Name = name;
      IP = ip;
      Rack = rack;
      Slot = slot;
    }

    //------------------------------------------------------------------------------
    // Check error (simply writes an header)
    //------------------------------------------------------------------------------
    public bool Check(int Result, string FunctionPerformed)
    {
      Console.WriteLine();
      Console.WriteLine("+-----------------------------------------------------");
      Console.WriteLine("| " + FunctionPerformed);
      Console.WriteLine("+-----------------------------------------------------");
      if (Result == 0)
      {
        int ExecTime = base.ExecTime();
        Console.WriteLine("| Result         : OK");
        Console.WriteLine("| Execution time : " + ExecTime.ToString() + " ms"); //+ Client.getex->ExecTime());
        Console.WriteLine("+-----------------------------------------------------");
      }
      else
      {
        Console.WriteLine("| ERROR !!! \n");
        if (Result < 0)
          Console.WriteLine("| Library Error (-1)\n");
        else
          Console.WriteLine("| " + base.ErrorText(Result));
        Console.WriteLine("+-----------------------------------------------------\n");
      }
      return Result == 0;
    }
    public bool con() {
      int res = ConnectTo(IP, Rack, Slot);
      if (Check(res, "UNIT Connection"))
      {
        int Requested = RequestedPduLength();
        int Negotiated = NegotiatedPduLength();
        Console.WriteLine("  Connected to   : " + IP + " (Rack=" + Rack.ToString() + ", Slot=" + Slot.ToString() + ")");
        Console.WriteLine("  PDU Requested  : " + Requested.ToString());
        Console.WriteLine("  PDU Negotiated : " + Negotiated.ToString());
      }
      return res == 0;
    }
  }
}
