using System;
using System.Collections.Generic;
using System.Linq;
using FMLib.ExtensionMethods;
using Utils;

namespace FMLib.Collections
{
  /// <summary>
  /// Class representing mapping strings values to T element
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class MapString<T> where T : class
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
    private Flag m_isDisposed;

    /// <summary>
    /// Dispose unmanaged resources
    /// </summary>
    public void Dispose()
    {
      if (m_isDisposed.CheckThenSet()) { return; }

      m_map.Clear();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public MapString()
    {
      // nothing to do here
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Adds value to map
    /// </summary>
    public void Add(string name, T value)
    {
      if (name.IsNullOrWhiteSpace()) { return; }
      if (value == null) { return; }
      m_map.Add((name, value));
    }

    /// <summary>
    /// Removes value from map
    /// </summary>
    public void Remove(string name, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
      if (name.IsNullOrWhiteSpace()) { return; }
      int index = m_map.FindIndex(x => x.Name.EQUAL(name, comparison));
      if (index != -1) { m_map.RemoveAt(index); }
    }

    /// <summary>
    /// Gets value from map
    /// </summary>
    public bool GetValue(string name, out T value, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
      value = null;
      if (name.IsNullOrWhiteSpace()) { return false; }
      int index = m_map.FindIndex(x => x.Name.Equals(name, comparison));
      if (index == -1) { return false; }
      value = m_map[index].Value;
      return true;
    }

    /// <summary>
    /// Returns collection of values
    /// </summary>
    public IEnumerable<T> Values => new List<T>(m_map.Select(x => x.Value));

    /// <summary>
    /// Returns collection of values
    /// </summary>
    public IEnumerable<string> Keys => new List<string>(m_map.Select(x => x.Name));

    /// <summary>
    /// Clears data
    /// </summary>
    public void Clear() => m_map.Clear();

    /// <summary>
    /// To string
    /// </summary>
    public override string ToString()
    {
      return string.Join("\n\t", m_map.Select(x => $"{x.Name}:{x.Value.ToString()}"));
    }

    #endregion

  }
}