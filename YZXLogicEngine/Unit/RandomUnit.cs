using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Common.Threading;

using Extensions;

namespace YZXLogicEngine.Unit
{
  /// <summary>
  /// 随机数模块
  /// </summary>
  public class RandomUnit : YZXUnit
  {   
    Random random;
    public ushort[] Mins { get; set; }
    public ushort[] Maxs { get; set; }

    public RandomUnit()
    {
      Init();
    }

    public override void Init()
    {
      bits = new List<bool>(Length);
      ReadLock = new OrderedLock();
      random = new Random();
      Mins = new ushort[Length];
      Maxs = new ushort[Length];

      for (int i = 0; i < Length; i++)
      {
        Maxs[i] = 65535;
      }
    }

    public RandomUnit(string name, ushort length=100)
      :base(name,length)
    {
      Init();
    }

    #region YZXUnit  
    public override List<bool> UpdateBits()
    {
      lock (ReadLock)
      {

        RecodeUpdatetimes();

        bits.Clear();
        for (int i = 0; i < Length; i++)
        {
          ushort min = Mins[i];
          ushort max = Maxs[i];
          ushort randomint = (ushort)random.Next(min, max);
          bool[] rbits = randomint.ToBoolArray();
          for (int j = 0; j < 16; j++)
          {
            bits.Add(rbits[j]);
          }
        }
        return bits;
      }
    }
    #endregion YZXUnit
  }
}
