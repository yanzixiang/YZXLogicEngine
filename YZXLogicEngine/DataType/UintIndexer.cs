using System;

namespace YZXLogicEngine
{
  /// <summary>
  /// Uint索引器
  /// </summary>
  public class UintIndexer
  {
    private YZXCPUMemory Mermory;
    public UintIndexer(YZXCPUMemory m)
    {
      Mermory = m;
    }

    public YZXUint this[int unitIndex]
    {
      get
      {
        if (unitIndex <= Mermory.B.Count - 1)
        {
          YZXUint w = new YZXUint(Mermory, unitIndex);
          return w;
        }

        throw new NotImplementedException();
      }
    }
    public override string ToString()
    {
      string s = string.Format("{0}{1}", Mermory.Name, "Uint");
      return s;
    }
  }
}
