using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Common.Collections;

using YZXLogicEngine.Task;
using YZXLogicEngine.UDT;

namespace YZXLogicEngine
{
  public enum MermoryType
  {
    Read,
    Write,
    S7VPLCInput,
    S7VPLCOutput
  }

  /// <summary>
  /// CPU内存区域
  /// 相当于西门子PLC的一个DB块
  /// </summary>
  public class YZXCPUMemory
  {
    public string Name { get; private set; }

    public MermoryType MermoryType { get; private set; } = MermoryType.Read;

    public YZXCPU CPU;///所属CPU
    
    public readonly int Length;   //存储区域字长度
    public List<YZXBit> B{get;private set;}        //实际位存储
    public List<YZXUshort> Ushorts { get; private set; }
    public List<YZXUint> Uints { get; private set; }
    public UshortIndexer Ushort { get; private set; }      //Ushort索引器
    public UintIndexer Uint { get; private set; }          //Uint索引器

    //public Dictionary<YZXUnit,Queue<>>

    #region 数据个数
    public int ByteCount
    {
      get
      {
        return B.Count / 8;
      }
    }

    public int UshortCount
    {
      get
      {
        return B.Count / 16;
      }
    }

    public int UintCount
    {
      get
      {
        return B.Count / 32;
      }
    }
    #endregion 数据个数

    public YZXCPUMemory() { }

    public YZXCPUMemory(string name,int length = 100)
    {
      Name = name;

      Length = length;
      FillEmptyBits(length);
      Ushort = new UshortIndexer(this);
      Uint = new UintIndexer(this);
    }

    //默认索引到字
    public YZXUshort this[int WIndex]
    {
      get
      {
        return Ushort[WIndex];
      }
    }

    /// <summary>
    /// 映射模块到内存区域的某一个部分
    /// 信息记录在每个YZXBit上
    /// </summary>
    /// <param name="begin">起始</param>
    /// <param name="length">长度</param>
    /// <param name="unit">模块</param>
    public void Map(ushort begin, ushort length, YZXUnit unit)
    {
      for (ushort i = 0; i < length; i++)
      {
        YZXUshort us = this[begin + i];
        us.Unit = unit;
        us.UShortIndexInUnit = i;

        for (int j = 0; j < 16; j++)
        {
          YZXBit b = us[j];
          b.Unit = unit;
          b.UShortIndexInUnit = i;
          b.BitIndexInUnit = j;
        }
      }
    }

    /// <summary>
    /// 更新某一区域内的数据
    /// 每个模块对应的定时任务内执行
    /// </summary>
    /// <param name="range">区域范围</param>
    /// <param name="bits">数据</param>
    public void UpdateBits(int start,int end, List<bool> bits)
    {
      int length = end - start + 1;

      if (bits.Count != length * 16)
      {
        return;
      }

      for (int i = 0; i < length; i++)
      {
        for (int j = 0; j < 15; j++)
        {
          int Findex = i * 16 + j;
          int Tindex = (start + i) * 16 +j;
          B[Tindex].Value = bits[Findex];
        }
      }
    }

    public void FillEmptyBits(int length = 1)
    {
      B = new List<YZXBit>(length * 16);
      for (int i = 0; i < length * 16; i++)
      {
        B.Add(new YZXBit(this));
      }

      Ushorts = new List<YZXUshort>(length);
      for (ushort i = 0; i < length;i++ )
      {
        Ushorts.Add(new YZXUshort(this, i));
      }

      Uints = new List<YZXUint>(length / 2);
      for (int i = 0; i < length / 2; i++)
      {
        Uints.Add(new YZXUint(this, i * 2));
      }
    }

    #region UDT
    public YZXUDT InitUDT(YZXUDTTypes type,YZXCPU cpu, string addressbase){
      switch(type){
        case YZXUDTTypes.UnitStatus:
          YZXUnitStatus status = new YZXUnitStatus();
          status.CPU = cpu;
          status.AddressBase = 
            new YZXAddress(string.Format("{0}.{1}", Name, addressbase));
          status.BuildMemberAddress();

          return status;
        default:
          return null;
      }
    }
    #endregion UDT
  }
}
