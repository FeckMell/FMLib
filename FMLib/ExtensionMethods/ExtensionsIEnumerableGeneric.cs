using System.Linq;

namespace System.Collections.Generic
{
  /// <summary>
  /// <see cref="IEnumerable{T}"/> extensions
  /// </summary>
  public static class ExtensionsIEnumerableGeneric
  {
    /// <summary>
    /// <para/>Makes <see cref="string.Join(string, IEnumerable{string})"/> an extension method.
    /// <para/>Do not throw.
    /// </summary>
    /// <param name="source"> Collection to apply <see cref="string.Join(string, IEnumerable{string})"/> to. If null - returns <see cref="string.Empty"/></param>
    /// <param name="separator"> Separator between elements of <paramref name="source"/>. If null - uses <see cref="string.Empty"/> separator.</param>
    /// <returns> String consists of elements of <paramref name="source"/> splitted with <paramref name="separator"/></returns>
    public static string StrJoin<T>(this IEnumerable<T> source, string separator)
    {
      if (source == null) { return string.Empty; }
      return string.Join(separator ?? string.Empty, source);
    }

    /// <summary>
    /// <para/>Provides extension method of adding range of elements for all generic collections.
    /// <para/>Do not throw.
    /// </summary>
    /// <param name="list">Collection to add to. If null - returns new <see cref="List{T}"/> with provided <paramref name="values"/></param>
    /// <param name="values">Values to add.</param>
    public static ICollection<T> AddRange<T, S>(this ICollection<T> list, params S[] values) where S : T
    {
      if (list == null) { list = new List<T>(); }
      foreach (var e in values) { list.Add(e); }
      return list;
    }

    /// <summary>
    /// <para/>Adds ForEach extension method.
    /// <para/>Throws if <paramref name="action"/> invocation throws.
    /// </summary>
    /// <param name="source">Collection to loop through. If null - return null.</param>
    /// <param name="action">Action that will be invoked for each element in <paramref name="source"/>. If null - returns <paramref name="source"/></param>
    /// <returns>Returns <paramref name="source"/> unchanged</returns>
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
      return source.ForEachA(action);
    }

    /// <summary>
    /// <para/>Invokes provided function on each element and returns list of results.
    /// <para/>Throws if <paramref name="func"/> invocation throws.
    /// </summary>
    /// <param name="source">Collection to loop through. If null - return empty <see cref="List{K}"/>.</param>
    /// <param name="func">Function that will be invoked for each element in <paramref name="source"/>. If null - returns empty <see cref="List{K}"/></param>
    /// <returns>Returns <see cref="List{K}"/> of results of <paramref name="func"/> invocations on <paramref name="source"/></returns>
    public static List<K> ForEach<T, K>(this IEnumerable<T> source, Func<T, K> func)
    {
      return source.ForEachF(func);
    }

    /// <summary>
    /// <para/>Converts <see cref="IEnumerable{byte}"/> ({0xff, 0xff, 0xff}) to HEX string ("FF FF FF")
    /// <para/>Do not throw.
    /// </summary>
    /// <param name="bytes">Array to convert. If null - returns <see cref="string.Empty"/></param>
    public static string BytesToHex(this IEnumerable<byte> bytes)
    {
      if (bytes == null) { return string.Empty; }
      return BitConverter.ToString(bytes.ToArray());
    }

    #region Private methods

    /// <summary>
    /// <para/>Made separated from <see cref="ExtensionsIEnumerableGeneric.ForEach{T}(IEnumerable{T}, Action{T})."/> to have ability to call to <see cref="ExtensionsIEnumerableGeneric.ForEachA{T}(IEnumerable{T}, Action{T})"/> directly.
    /// <para/>Adds ForEach extension method.
    /// <para/>Throws if <paramref name="action"/> invocation throws.
    /// </summary>
    /// <param name="source">Collection to loop through. If null - return null.</param>
    /// <param name="action">Action that will be invoked for each element in <paramref name="source"/>. If null - returns <paramref name="source"/></param>
    /// <returns>Returns <paramref name="source"/> unchanged</returns>
    private static IEnumerable<T> ForEachA<T>(this IEnumerable<T> source, Action<T> action)
    {
      if (source == null) { return source; }
      if (action == null) { return source; }
      foreach (T e in source) { action(e); }
      return source;
    }

    /// <summary>
    /// <para/>Made separated from <see cref="ExtensionsIEnumerableGeneric.ForEachF{T, K}(IEnumerable{T}, Func{T, K})"/> to have ability to call to <see cref="ExtensionsIEnumerableGeneric.ForEachF{T, K}(IEnumerable{T}, Func{T, K})"/> directly.
    /// <para/>Invokes provided function on each element and returns list of results.
    /// <para/>Throws if <paramref name="func"/> invocation throws.
    /// </summary>
    /// <param name="source">Collection to loop through. If null - return empty <see cref="List{K}"/>.</param>
    /// <param name="func">Function that will be invoked for each element in <paramref name="source"/>. If null - returns empty <see cref="List{K}"/></param>
    /// <returns>Returns <see cref="List{K}"/> of results of <paramref name="func"/> invocations on <paramref name="source"/></returns>
    private static List<K> ForEachF<T, K>(this IEnumerable<T> source, Func<T, K> func)
    {
      List<K> result = new List<K>();
      if (source == null) { return result; }
      if (func == null) { return result; }
      foreach (T e in source) { result.Add(func(e)); }
      return result;
    }

    #endregion

  }
}