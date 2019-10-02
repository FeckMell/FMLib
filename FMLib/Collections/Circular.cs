using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FMLib.Collections
{

  /// <summary>
  /// Class representing circular storage.
  /// If It's element count exceeds capacity - deletes oldest element
  /// </summary>
  [JsonObject(MemberSerialization = MemberSerialization.Fields)]
  public class Circular<T> : IEnumerable<T>
  {

    #region Fields

    /// <summary>
    /// Capacity
    /// </summary>
    private int m_capacity;

    /// <summary>
    /// Data storage
    /// </summary>
    private List<T> m_data;

    /// <summary>
    /// Element count in storage
    /// </summary>
    public int Count => m_data.Count;

    /// <summary>
    /// Returns first element
    /// </summary>
    public T Top => m_data.Count == 0 ? default(T) : m_data[m_data.Count - 1];

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Constructor. Sets capacity
    /// </summary>
    public Circular(int capacity)
    {
      if (capacity <= 0) { capacity = 1; }
      m_capacity = capacity;
      m_data = new List<T>(m_capacity);
    }

    #endregion

    /// <summary>
    /// Adds element to end of storage. If oversizes - removes oldest
    /// </summary>
    public void Push(T value)
    {
      if (m_data.Count + 1 > m_capacity) { m_data.RemoveAt(0); }
      m_data.Add(value);
    }

    /// <summary>
    /// Returns first element and deletes it from storage
    /// </summary>
    public T Pop()
    {
      if (m_data.Count == 0) { return default(T); }
      T result = m_data[0];
      m_data.RemoveAt(0);
      return result;
    }

    /// <summary>
    /// Clears data in storage
    /// </summary>
    public void Clear()
    {
      m_data.Clear();
    }

    /// <summary>
    /// Puts given value on top of container.
    /// </summary>
    public void BringOnTop(T value)
    {
      var found = m_data.FindIndex(x => x.Equals(value));
      if (found != -1) { m_data.RemoveAt(found); }
      Push(value);
    }

    /// <summary>
    /// <see cref="IEnumerable{T}"/> implementation
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
      return m_data.GetEnumerator();
      //var list = new List<T>(m_data);
      //list.Reverse();
      //var t = list.GetEnumerator();
      //return t;
    }

    /// <summary>
    /// <see cref="IEnumerable"/> implementation
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
  }
}