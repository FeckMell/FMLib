using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Utils
{
  /// <summary>
  /// Class for inner Simulator exceptions. Mostly used to separate exceptions thrown by user code and system functions
  /// </summary>
  [Serializable]
  public class ExpectedException : Exception
  {

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectedException(string message, [CallerMemberName] string methodName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
      : base($"{Path.GetFileName(filePath)}: line={lineNumber}: {methodName}: {message}")
    {
      // nothing to do here
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ExpectedException(string message, Exception innerException, [CallerMemberName] string methodName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
      : base($"{Path.GetFileName(filePath)}: line={lineNumber}: {methodName}: {message}", innerException)
    {
      // nothing to do here
    }

    /// <summary>
    /// ToString
    /// </summary>
    public override string ToString() => Message + (InnerException == null ? "" : $"\tInner exception: {InnerException.Message}");
  }
}