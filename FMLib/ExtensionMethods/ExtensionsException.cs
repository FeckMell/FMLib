namespace System
{
  /// <summary>
  /// Extension methods for <see cref="Exception"/>
  /// </summary>
  public static class ExtensionsException
  {
    /// <summary>
    /// <para/>Stacks all exceptions nicely
    /// <para/>Do not throw
    /// </summary>
    public static string FullInfo(this Exception ex)
    {
      var message = string.Empty;
      var stack = string.Empty;
      int depth = 0;
      while (ex != null)
      {
        message += $"{depth}) {ex.Message}\n";
        stack += $"{depth}) {ex.StackTrace}\n\n";
        depth++;
      }
      return $"Messages:\n{message.ToString().Trim()}\n\nStacks:\n{stack.ToString().Trim()}";
    }
  }
}