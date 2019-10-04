using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Utils.ExtensionMethods
{
  /// <summary>
  /// Extensions to work with <see cref="string"/>
  /// </summary>
  public static class ExtensionsString
  {
    /// <summary>
    /// <para/>Make <see cref="string.IsNullOrWhiteSpace(string)"/> extension method.
    /// <para/>Do not throw.
    /// </summary>
    public static bool IsNullOrWhiteSpace(this string target)
    {
      return string.IsNullOrWhiteSpace(target);
    }

    /// <summary>
    /// <para/>Make <see cref="string.IsNullOrEmpty(string)"/> extension method.
    /// <para/>Do not throw.
    /// </summary>
    public static bool IsNullOrEmpty(this string target)
    {
      return string.IsNullOrEmpty(target);
    }

    /// <summary>
    /// <para/>Make <see cref="string.Equals(string, string, StringComparison)"/> extension method.
    /// <para/>Do not throw.
    /// </summary>
    public static bool EQUAL(this string first, string second, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
      return string.Equals(first, second, comparison);
    }

    /// <summary>
    /// <para/>Replaces illegal chars in file name with '$'.
    /// <para/>Do not throw.
    /// </summary>
    /// <param name="target">If null - returns <see cref="string.Empty"/>.</param>
    /// <param name="replacement">Replacement symbol. If null - uses $</param>
    public static string RemoveInvalidFilenameChars(this string target, char replacement = '$')
    {
      if (target == null) { return string.Empty; }
      StringBuilder result = new StringBuilder(target);
      Path.GetInvalidFileNameChars().ForEach(x => result.Replace(x, replacement));
      return result.ToString();
    }

    /// <summary>
    /// <para/>Copies string n times.
    /// <para/>Do not throw.
    /// </summary>
    /// <param name="target">If null - returns <see cref="string.Empty"/>.</param>
    public static string Repeat(this string target, int count)
    {
      if (target == null) { return string.Empty; }
      if (count < 1) { return string.Empty; }
      return string.Concat(Enumerable.Repeat(target, count));
    }

    /// <summary>
    /// <para/>Substring from <paramref name="beginIndex"/> to <paramref name="endIndex"/>. Last element is included.
    /// <para/>Do not throw.
    /// </summary>
    public static string Substring(this string data, int beginIndex, int endIndex, bool dummy)
    {
      if (data == null) { return string.Empty; }
      if (beginIndex < 0 || endIndex >= data.Length || beginIndex > endIndex) { return string.Empty; }
      return data.Substring(beginIndex, endIndex - beginIndex + 1);
    }

    /// <summary>
    /// Calculates the Levenshtein distance
    /// </summary>
    /// <param name="first">Value 1. If null - returns <see cref="int.MaxValue"/></param>
    /// <param name="second">Value 2. If null - returns <see cref="int.MaxValue"/></param>
    /// <returns>The Levenshtein distance</returns>
    public static int LevenshteinDistance(this string first, string second)
    {
      int defaultResult = int.MaxValue;
      if (first == null || second == null) { return defaultResult; }

      var matrix = new int[first.Length + 1, second.Length + 1];
      for (int x = 0; x <= first.Length; ++x)
        matrix[x, 0] = x;
      for (int x = 0; x <= second.Length; ++x)
        matrix[0, x] = x;

      for (int x = 1; x <= first.Length; ++x)
      {
        for (int y = 1; y <= second.Length; ++y)
        {
          int cost = first[x - 1] == second[y - 1] ? 0 : 1;
          matrix[x, y] = new int[] { matrix[x - 1, y] + 1, matrix[x, y - 1] + 1, matrix[x - 1, y - 1] + cost }.Min();
          if (x > 1 && y > 1 && first[x - 1] == second[y - 2] && first[x - 2] == second[y - 1])
            matrix[x, y] = new int[] { matrix[x, y], matrix[x - 2, y - 2] + cost }.Min();
        }
      }

      return matrix[first.Length, second.Length];
    }

    ///// <summary>
    ///// Gets strings between two values
    ///// </summary>
    //public static List<string> StringsBetween(this string data, string start, string end)
    //{
    //  List<string> result = new List<string>();
    //  if (data.IsNullOrWhiteSpace() || start.IsNullOrWhiteSpace() || end.IsNullOrWhiteSpace()) { return result; }
    //  int startIndex = 0;
    //  int endIndex = 0;
    //  while (!data.IsNullOrWhiteSpace())
    //  {
    //    if ((startIndex = data.IndexOf(start, StringComparison.OrdinalIgnoreCase)) == -1) { return result; }
    //    data = data.Substring(startIndex); // cut beginning
    //    if ((endIndex = data.IndexOf(end, StringComparison.OrdinalIgnoreCase)) == -1) { return result; }
    //    result.Add(data.Substring(0, endIndex + end.Length)); // add found part
    //    data = data.Substring(endIndex + end.Length); // cut found path
    //  }
    //  return result;
    //}
  }
}