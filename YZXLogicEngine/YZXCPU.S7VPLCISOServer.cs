using System;
using System.Net;
using System.Collections.Generic;

using Extensions;
using System.Threading;

namespace YZXLogicEngine
{
  partial class YZXCPU
  {
    public IsoToS7online S7VPLCISOServer;
    public int S7VPLCISOServerPort = 8102;
    public string S7VPLCISOServerName = "S7VPLC";

    public List<byte[]> S7VPLCTSAPS = new List<byte[]>()
    {
    new byte[] { 0x01, 0x01 },
    new byte[] { 0x02, 0x01 },
    new byte[] { 0x03, 0x01 }
    };
    public void StartS7VPLCISOServer(string VPLCIP)
    {
      try
      {
        S7VPLCISOServer = new IsoToS7online(false, S7VPLCISOServerPort);
        IPAddress localhost = "127.0.0.1".ToIPAddress();
        IPAddress address = VPLCIP.ToIPAddress();
        S7VPLCISOServer.start(S7VPLCISOServerName, localhost, S7VPLCTSAPS, address, 0, 1);
      }
      catch (Exception)
      {

        throw;
      }
    }
  }
}
