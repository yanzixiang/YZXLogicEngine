using System;

using OpcRcw.Da;

namespace YZXLogicEngine.Extensions
{
  public static class OPCExtensions
  {
    /// <summary>
    /// Converts FILETIME to DateTime
    /// </summary>
    private static DateTime ToDateTime(FILETIME ft)
    {
      long highbuf = ft.dwHighDateTime;
      long buffer = (highbuf << 32) + ft.dwLowDateTime;
      return DateTime.FromFileTimeUtc(buffer);
    }
  }
}
