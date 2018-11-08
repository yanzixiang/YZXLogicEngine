using System;

using Extensions;

namespace YZXLogicEngine
{
  /// <summary>
  /// Byte索引器
  /// </summary>
  public class ByteIndexer
  {
    private YZXCPUMemory Mermory;
    public ByteIndexer(YZXCPUMemory m)
    {
      Mermory = m;
    }

    public YZXByte this[int byteIndex]
    {
      get
      {
        if (byteIndex.InRange(1,Mermory.ByteCount))
        {
          YZXByte b = new YZXByte(Mermory, byteIndex);
          return b;
        }
        throw new InvalidOperationException();
      }
    }
    public override string ToString()
    {
      string s = string.Format("{0}{1}", Mermory.Name, "Byte");
      return s;
    }
  }
}
