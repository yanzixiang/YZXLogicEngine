using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YZXLogicEngine
{
  class OmronUDP
  {
    /// <summary>
    ///  生成读取命令
    /// </summary>
    /// <param name="byPlcDot">plc节点号</param>
    /// <param name="byLocalDot">本机节点号</param>
    /// <param name="index">读的地址的 数据区 例如：D600</param>
    /// <param name="WordNumber">读取的 数据的个数 15 表示 D600-D615</param>
    /// <param name="addressType">读取的地址 plc D区:0x82 plc H区:0xB2</param>
    /// <returns>生成的命令</returns>
    public static byte[] CmdOmlonReadData(byte byPlcDot, 
      byte byLocalDot, 
      int index, 
      int WordNumber, 
      OMLONADDRESS addressType)
    {
      byte[] cmdData = new byte[32];
      cmdData[0] = 0x80;
      cmdData[1] = 0x00;
      cmdData[2] = 0x02;
      cmdData[3] = 0x00;       //PLC网络号

      cmdData[4] = byPlcDot;   //PLC节点号
      cmdData[5] = 0x00;       //PLC单元号

      cmdData[6] = 0x00;       //上位机网络号
      cmdData[7] = byLocalDot; //上位机节点号
      cmdData[8] = 0x00;

      cmdData[9] = 0x03;       //SID

      cmdData[10] = 0x01;      //读命令
      cmdData[11] = 0x01;      //读命令

      cmdData[12] = (byte)addressType;

      cmdData[13] = (byte)(index / 256);    //起始地址
      cmdData[14] = (byte)(index % 256);    //起始地址

      cmdData[15] = 0x00;

      cmdData[16] = (byte)(WordNumber / 256);  //读取字数
      cmdData[17] = (byte)(WordNumber % 256);  //读取字数

      return cmdData;

    }

    /// <summary>
    /// 写OMLON PLC地址数据
    /// </summary>
    /// <param name="cmdData">生成的命令</param>
    /// <param name="byPlcDot">plc 节点</param>
    /// <param name="byLocalDot">本机 节点</param>
    /// <param name="index">地址的 数据区 例如：D600</param>
    /// <param name="WriteNumber">个数</param>
    /// <param name="writeValue">需要写入的数据</param>
    /// <param name="addressType">地址 plc D区:0x82 plc H区:0xB2</param>
    /// <returns></returns>
    public static int CmdOmlonWriteData(byte[] cmdData, 
      byte byPlcDot, 
      byte byLocalDot, 
      int index, 
      int WriteNumber, 
      byte[] writeValue, 
      OMLONADDRESS addressType)
    {
      int commandLength = 0;

      cmdData[0] = 0x80;
      cmdData[1] = 0x00;
      cmdData[2] = 0x02;
      cmdData[3] = 0x00;       //PLC网络号

      cmdData[4] = byPlcDot;   //PLC节点号
      cmdData[5] = 0x00;       //PLC单元号

      cmdData[6] = 0x00;       //上位机网络号
      cmdData[7] = byLocalDot; //上位机节点号
      cmdData[8] = 0x00;

      cmdData[9] = 0x03;       //SID

      cmdData[10] = 0x01;      //写命令
      cmdData[11] = 0x02;      //写命令

      cmdData[12] = (byte)addressType;

      cmdData[13] = (byte)(index / 256);    //起始地址
      cmdData[14] = (byte)(index % 256);    //起始地址

      cmdData[15] = 0x00;

      cmdData[16] = (byte)(WriteNumber / 256);  //写的个数
      cmdData[17] = (byte)(WriteNumber % 256);  //写的个数

      for (int n = 0; n < WriteNumber * 2; n++)
      {
        cmdData[18 + n] = writeValue[n];
      }

      commandLength = 18 + 2 * WriteNumber;

      return commandLength;

    }

    private Queue<UdpData> sendDataList = new Queue<UdpData>();   //发送数据缓冲区


    public void SendData()
    {
      try
      {
        udpSend.Connect(remoteEP);
      }
      catch
      {
        MessageBox.Show("连接错误 " + remoteEP.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return;
      }
      connectSuccess = true;
      byte[] writeValue = new byte[16];
      Byte[] sendBytes = { 1, 1 };

      while (true)
      {
        if (cts.Token.IsCancellationRequested)
        {
          break;
        }

        bool sendBuffer = false;
        // 上锁线程安全
        lock (sendDataLock)
        {
          if (sendDataList.Count > 0) //如果队列里面有需要发送的数据　则发送
          {
            UdpData getData = sendDataList.Dequeue();
            sendBuffer = true;
            udpSend.BeginSend(getData.buffer, getData.length, new AsyncCallback(SendCallback), udpSendState);
          }
        }

        if (sendBuffer == false) // 发送读取的命令
        {
          udpSend.BeginSend(readAgvPlcCommand, 18, readCallback, udpSendState);
          ReceiveMessages();
        }
        if (sendBuffer == false)
          Thread.Sleep(4000);
        else
          Thread.Sleep(100);
      }
    }

    public void addSendBuffer(ReadWriteType readWriteType, byte[] data)
    {
      UdpData pData = new UdpData();
      if (readWriteType == ReadWriteType.WRITE_AGV_D600)
      {
        pData.length = CmdOmlonWriteData(pData.buffer, byPlcDot, localDot, 601, 1, data, OMLONADDRESS.D);
      }
      Console.WriteLine(pData.buffer);
      //Queue<T>  不是线程安全的
      lock (sendDataLock)
      {
        sendDataList.Enqueue(pData);
      }
    }
    // 发送回调函数
    public void SendCallback(IAsyncResult iar)
    {
      UdpState udpState = iar.AsyncState as UdpState;
      if (iar.IsCompleted)
      {
        udpState.udpClient.EndSend(iar);
      }
    }
    // 接收函数
    public void ReceiveMessages()
    {
      lock (this)
      {
        try
        {
          udpSend.BeginReceive(receiveCallback, udpReceiveState);
        }
        catch (SocketException e)
        {
          string strException = string.Format("SocketException:{0}", e);
        }

        Thread.Sleep(100);
      }
    }
    //读数据接收回调函数
    public void ReceiveCallback(IAsyncResult iar)
    {
      UdpState udpState = iar.AsyncState as UdpState;

      if (iar.IsCompleted)
      {
        Byte[] receiveBytes = udpState.udpClient.EndReceive(iar, ref udpReceiveState.ipEndPoint);

        if (receiveBytes.Length >= 44) // 读数据返回
        {
          DataForm mainform = g_em as DataForm;

          if (bUpdateFirst == true)
          {
            mi = new MyInvoke(UpdateForm);
            bUpdateFirst = false;
          }
          if (mainform.InvokeRequired)
          {
            mainform.BeginInvoke(mi, receiveBytes);
            //mainform.BeginInvoke((MethodInvoker)(() =>
            //{
            //    string strData = "";
            //    DataForm mainForm = g_em as DataForm;
            //    strData = "";
            //    for (int n = 0; n < receiveBytes.Length; n++)
            //        strData = strData + " " + (receiveBytes[n]).ToString("X2");
            //    mainForm.lstData.Items.Insert(0, strData);
            //    strData = DateTime.Now.ToString("HH:mm:ss ");
            //    mainForm.lstData.Items.Insert(0, strData);
            //    Console.WriteLine(strData);
            //}));
          }
          Console.WriteLine();
        }
        else if (receiveBytes.Length >= 10) // 写数据返回
        {
          readWriteType = ReadWriteType.READ_AGV_PLC; ////恢复为读命令
        }
      }
    }
  }
}
