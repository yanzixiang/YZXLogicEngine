using System;

using Extensions;

namespace YZXLogicEngine
{
  /// <summary>
  /// Ushort索引器
  /// </summary>
  public class UshortIndexer
  {
    private YZXCPUMemory Mermory;
    public UshortIndexer(YZXCPUMemory m)
    {
      Mermory = m;
    }

    public YZXUshort this[int ushortIndex]
    {
      get
      {
        if (ushortIndex.InRange(0, Mermory.UshortCount - 1, true))
        {
          YZXUshort w = Mermory.Ushorts[ushortIndex];
          return w;
        }
        throw new InvalidOperationException();
      }
    }

    public override string ToString()
    {
      string s = string.Format("{0}{1}", Mermory.Name, "Ushort");
      return s;
    }
  }
}
