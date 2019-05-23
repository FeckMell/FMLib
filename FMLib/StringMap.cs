using System;
using System.Collections.Generic;
using System.Linq;
namespace FMLib
{
  /// <summary>
  /// Class representing mapping strings values to <see cref="T"/> element
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class StringMap<T> where T : class
  {

    #region Fields

    /// <summary>
    /// Container for stored values
    /// </summary>
    private List<(string Name, T Value)> m_map = new List<(string, T)>();

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Determines if object was disposed
    /// </summary>
    private bool m_isDisposed = false;

    /// <summary>
    /// Dispose unmanaged resources
    /// </summary>
    public void Dispose()
    {
      if (m_isDisposed) { return; }
      m_isDisposed = true;

      m_map.Clear();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public StringMap()
    {
      // nothing to do here
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Adds value to map
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    public void Add(string name, T value)
    {
      if (string.IsNullOrWhiteSpace(name)) { return; }
      if (value == null) { return; }
      m_map.Add((name, value));
    }

    /// <summary>
    /// Removes value from map
    /// </summary>
    /// <param name="name"></param>
    public void Remove(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
      if (string.IsNullOrWhiteSpace(name)) { return; }
      int index = m_map.FindIndex(x => x.Name.Equals(name, comparison));
      if (index != -1) { m_map.RemoveAt(index); }
    }

    /// <summary>
    /// Gets value from map
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool GetValue(string name, out T value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
      value = null;
      if (string.IsNullOrWhiteSpace(name)) { return false; }
      int index = m_map.FindIndex(x => x.Name.Equals(name, comparison));
      if (index == -1) { return false; }
      value = m_map[index].Value;
      return true;
    }

    #endregion
  }
}