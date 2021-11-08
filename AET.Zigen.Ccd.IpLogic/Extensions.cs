using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace AET.Zigen.Ccd.IpLogic {
  internal static class Extensions {
    public static int SafeParseInt(this string value) {
      try {
        return int.Parse(value);
      }
      catch {
        return 0;
      }
    }
  }
}