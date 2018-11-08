using System;

using Extensions;

namespace YZXLogicEngine
{
  public class YZXAddress
  {
    public static char[] AddressDelimiterChars = { '.' };
    public string mIndex { get; private set; }
    public int wIndex { get; private set; }
    public short bIndex { get; private set; }

    public YZXAddress()
    {

    }

    public YZXAddress(string mIndex, int wIndex = 0, short bIndex = 0)
    {
      this.mIndex = mIndex;
      this.wIndex = wIndex;
      this.bIndex = bIndex;
    }
    public YZXAddress(string address)
    {
      string[] ss = address.Split(AddressDelimiterChars);

      

      if (ss.Length > 0)
      {
        mIndex = ss[0];
      }

      if (ss.Length > 1)
      {
        wIndex = ss[1].ToShort();
      }

      if (ss.Length > 2)
      {
        bIndex = ss[2].ToShort();
      }
    }

    public override string ToString()
    {
      string s = string.Format("{0}.{1}.{2}", mIndex, wIndex, bIndex);
      return s;
    }

    public YZXAddress Offset(string offset)
    {
      string[] ss = offset.Split(AddressDelimiterChars);
      if (ss.Length.InRange(1, 2, true))
      {
        YZXAddress na = new YZXAddress(mIndex, wIndex + ss[0].ToShort());

        if (ss.Length == 2)
        {
          int bitoffset = ss[1].ToShort() + bIndex;

          int rem = 0;
          int div = Math.DivRem(bitoffset, 8, out rem);

          na.wIndex += div;
          na.bIndex = (short)rem;
        }
        return na;

      }
      else
      {
        throw new InvalidOperationException();
      }

    }

  }

  public static class AddressExtentions
  {
    public static string Offset(this string address, string offset)
    {
      string newAddress = "";
      YZXAddress a = new YZXAddress(address);
      YZXAddress na =  a.Offset(offset);
      newAddress = na.ToString();
      return newAddress;
    }
  }
}
