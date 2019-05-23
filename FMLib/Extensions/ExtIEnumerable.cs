using System;
using System.Collections.Generic;
using System.Linq;
namespace FMLib.Extensions
{
  /// <summary>
  /// Provides some extension methods for <see cref="IEnumerable{T}"/>
  /// </summary>
  public static class ExtIEnumerable
  {
    /// <summary>
    /// Invokes provided action on each element. May throw if action parameter throws.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
      if (source == null) { return source; }
      if (action == null) { return source; }
      foreach (T e in source) { action(e); }
      return source;
    }

    /// <summary>
    /// Invokes provided function on each element and returns list of results. May throw if action parameter throws.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    public static List<K> ForEach<T,K>(this IEnumerable<T> source, Func<T,K> func)
    {
      List<K> result = new List<K>();
      if (source == null) { return result; }
      if (func == null) { return result; }
      foreach (T e in source) { result.Add(func(e)); }
      return result;
    }
  }
}