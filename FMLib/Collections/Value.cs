namespace Utils.Collections
{
  /// <summary>
  /// Class for making any type nullable
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class Value<T>
  {
    /// <summary>
    /// Underlying value
    /// </summary>
    private T m_value;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="value"></param>
    private Value(T value)
    {
      m_value = value;
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString()
    {
      // Don't need to do null-check as m_value is always!=null due to implicit operator and private constructor
      return m_value.ToString();
    }

    /// <summary>
    /// Implicit conversion from <see cref="Value{T}"/> to <typeparamref name="T"/>
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator T(Value<T> value)
    {
      if (value == null) { return default(T); }
      return value.m_value;
    }

    /// <summary>
    /// Implicit conversion from <typeparamref name="T"/> to <see cref="Value{T}"/>
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator Value<T>(T value)
    {
      if (value == null) { return null; }
      return new Value<T>(value);
    }
  }

  /// <summary>
  /// Extensions methods for <see cref="Value{T}"/> that can't be added to class directly
  /// </summary>
  public static class ExtensionsValue
  {
    /// <summary>
    /// Gets underlying value for calling members of <typeparamref name="T"/>.
    /// Can return null if <paramref name="value"/> was null
    /// </summary>
    public static T Get<T>(this Value<T> value)
    {
      return (T)value;
    }
  }
}