using System;
using System.Linq;
using System.Collections.Generic;

namespace Inqlude.Database.Util {
  /// <summary>
  /// Various methods with random outputs. The randomization is NOT secure!
  /// </summary>
  public static class Chance {
    public static readonly string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GetString(int length) {
      var random = new Random();

      return new String(Enumerable.Repeat(Chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
  }
}