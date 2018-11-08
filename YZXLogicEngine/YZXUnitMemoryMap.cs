using System;

namespace YZXLogicEngine
{
  public class YZXUnitMemoryMap
  {
    public string Unit;
    public string Memory;
    public ushort Begin;
    public ushort Length;

    public YZXUnitMemoryMap(string unit, string memory, ushort begin =0, ushort length =100)
    {
      Unit = unit;
      Memory = memory;
      Begin = begin;
      Length = length;
    }
  }
}
