using System.Text;
using FMLib.ExtensionMethods;

namespace System.IO
{
  /// <summary>
  /// Extensions to work with <see cref="Stream"/>
  /// </summary>
  public static class ExtensionsStream
  {
    /// <summary>
    /// <para/>Takes all of the data in the stream and returns it as a string
    /// <para/>May throw: if <paramref name="input"/> is null or see exceptions in <see cref="Stream.CopyTo(Stream)"/> and <see cref="Encoding.GetString(byte[])"/>
    /// </summary>
    /// <param name="input">Input stream</param>
    /// <param name="encoding"> Encoding that the string should be in (defaults: An encoding for the operating system's current ANSI code page) </param>
    /// <returns>A string containing the content of the stream</returns>
    public static string ReadAllText(this Stream input, Encoding encoding = null)
    {
      return (encoding ?? Encoding.Default).GetString(input.ReadAllBinary());
    }

    /// <summary>
    /// <para/>Takes all of the data in the stream and returns it as an array of bytes.
    /// <para/>May throw: if <paramref name="input"/> is null or see exceptions in <see cref="Stream.CopyTo(Stream)"/>
    /// </summary>
    /// <param name="input">Input stream</param>
    /// <returns>A byte array</returns>
    public static byte[] ReadAllBinary(this Stream input)
    {
      if (input.IsOfType(out MemoryStream ms)) { return (input as MemoryStream).ToArray(); }

      using (ms = new MemoryStream())
      {
        input.CopyTo(ms);
        return ms.ToArray();
      }
    }
  }
}