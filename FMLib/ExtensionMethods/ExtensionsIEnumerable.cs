using System.Collections.Generic;

namespace System.Collections
{
  /// <summary>
  /// Extensions to work with <see cref="IEnumerable"/>
  /// </summary>
  public static class ExtensionsEnumerable
  {
    /// <summary>
    /// <para/>Makes <see cref="string.Join(string, IEnumerable)"/> an extension method.
    /// <para/>Do not throw.
    /// </summary>
    /// <param name="source"> Collection to apply <see cref="string.Join(string, IEnumerable)"/> to. If null - returns <see cref="string.Empty"/></param>
    /// <param name="separator"> Separator between elements of <paramref name="source"/>. If null - uses <see cref="string.Empty"/> separator.</param>
    /// <returns> String consists of elements of <paramref name="source"/> splitted with <paramref name="separator"/></returns>
    public static string StrJoin(this IEnumerable source, string separator)
    {
      if (source == null) { return string.Empty; }
      return string.Join(separator ?? string.Empty, source);
    }

    /// <summary>
    /// <para/>Adds ForEach extension method.
    /// <para/>Throws if <paramref name="action"/> invocation throws.
    /// </summary>
    /// <param name="source">Collection to loop through. If null - return null.</param>
    /// <param name="action">Action that will be invoked for each element in <paramref name="source"/>. If null - returns <paramref name="source"/></param>
    /// <returns>Returns <paramref name="source"/> unchanged</returns>
    public static IEnumerable ForEach(this IEnumerable source, Action<object> action)
    {
      return source.ForEachA(action);
    }

    /// <summary>
    /// <para/>Invokes provided function on each element and returns list of results.
    /// <para/>Throws if <paramref name="func"/> invocation throws.
    /// </summary>
    /// <param name="source">Collection to loop through. If null - return empty <see cref="List{T}"/>.</param>
    /// <param name="func">Function that will be invoked for each element in <paramref name="source"/>. If null - returns empty <see cref="List{T}"/></param>
    /// <returns>Returns <see cref="List{T}"/> of results of <paramref name="func"/> invocations on <paramref name="source"/></returns>
    public static IEnumerable ForEach<T>(this IEnumerable source, Func<object, T> func)
    {
      return source.ForEachF(func);
    }

    #region Private methods

    /// <summary>
    /// <para/>Adds ForEach extension method.
    /// <para/>Throws if <paramref name="action"/> invocation throws.
    /// </summary>
    /// <param name="source">Collection to loop through. If null - return null.</param>
    /// <param name="action">Action that will be invoked for each element in <paramref name="source"/>. If null - returns <paramref name="source"/></param>
    /// <returns>Returns <paramref name="source"/> unchanged</returns>
    private static IEnumerable ForEachA(this IEnumerable source, Action<object> action)
    {
      if (source == null) { return source; }
      if (action == null) { return source; }
      foreach (var e in source) { action(e); }
      return source;
    }

    /// <summary>
    /// <para/>Invokes provided function on each element and returns list of results.
    /// <para/>Throws if <paramref name="func"/> invocation throws.
    /// </summary>
    /// <param name="source">Collection to loop through. If null - return empty <see cref="List{T}"/>.</param>
    /// <param name="func">Function that will be invoked for each element in <paramref name="source"/>. If null - returns empty <see cref="List{T}"/></param>
    /// <returns>Returns <see cref="List{T}"/> of results of <paramref name="func"/> invocations on <paramref name="source"/></returns>
    private static IEnumerable ForEachF<T>(this IEnumerable source, Func<object, T> func)
    {
      List<T> result = new List<T>();
      if (source == null) { return result; }
      if (func == null) { return result; }
      foreach (var e in source) { result.Add(func(e)); }
      return result;
    }

    #endregion
  }
}