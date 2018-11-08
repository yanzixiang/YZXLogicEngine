using System.Net;
using System.Net.Sockets;

public class UdpState
{
  public UdpClient udpClient = null;
  public IPEndPoint ipEndPoint = null;
  public const int BufferSize = 1024;
  public byte[] buffer = new byte[BufferSize];
  public int counter = 0;
}
public class UdpData
{
  public byte[] buffer = new byte[1024];
  public int recvValue = 0;
  public int length = 0;
}