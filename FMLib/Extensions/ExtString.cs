using System;
using System.Collections.Generic;
using System.Linq;
namespace FMLib.Extensions
{
  /// <summary>
  /// Class that gives extension methods for string class
  /// </summary>
  public static class ExtString
  {
    /// <summary>
    /// Make <see cref="string.IsNullOrWhiteSpace(string)"/> Extension method
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsNullOrWhiteSpace(this string target) => string.IsNullOrWhiteSpace(target);

    /// <summary>
    /// Make <see cref="string.IsNullOrEmpty(string)"/> Extension method
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string target) => string.IsNullOrEmpty(target);

    /// <summary>
    /// Make <see cref="string.Copy(string)"/> Extension method
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public static string Copy(this string target) => string.Copy(target);
  }
}