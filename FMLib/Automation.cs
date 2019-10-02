using System;
using System.Threading;
using FMLib.ExtensionMethods;

namespace Utils
{
  /// <summary>
  /// Provides various methods for automated work with objects
  /// </summary>
  public static class Automation
  {
    /// <summary>
    /// Wraps provided actions to try-catch.
    /// </summary>
    /// <param name="action">action in try section</param>
    /// <param name="failAction">action in catch section</param>
    /// <returns>True - if didn't throw. False otherwise</returns>
    public static bool TryCatch(this Action action, Action<Exception> failAction = null)
    {
      try
      {
        action?.Invoke();
        return true;
      }
      catch (Exception ex)
      {
        failAction?.Invoke(ex);
        return false;
      }
    }

    /// <summary>
    /// Wraps provided actions to try-catch.
    /// </summary>
    /// <param name="action">action in try section</param>
    /// <param name="failAction">action in catch section</param>
    /// <param name="exception"> out exception </param>
    /// <returns>True - if didn't throw. False otherwise</returns>
    public static bool TryCatch(this Action action, out Exception exception, Action<Exception> failAction = null)
    {
      try
      {
        action?.Invoke();
        exception = null;
        return true;
      }
      catch (Exception ex)
      {
        exception = ex;
        failAction?.Invoke(exception);
        return false;
      }
    }

    /// <summary>
    /// <para/>Executes provided action with retries in same thread.
    /// <para/>Throws last exception if exceeded <paramref name="maxAttempts"/>. Also see parameters description.
    /// </summary>
    /// <param name="action">Action to repeat. Throws if null</param>
    /// <param name="maxAttempts">Amount of attempts to repeat. Throws if less than 1</param>
    /// <param name="retryDelay">Amount of milliseconds to wait between retries</param>
    public static void ExecuteWithRetry(Action action, int maxAttempts = 3, int retryDelay = 0)
    {
      action.ThrowIfNull(nameof(action));
      if (maxAttempts < 1) { throw new ArgumentException(nameof(maxAttempts)); }

      Exception exception = null;
      for (int i = 0; i < maxAttempts; i++)
      {
        if (Automation.TryCatch(action, out exception)) { return; }
        Thread.Sleep(retryDelay);
      }
      throw exception;
    }

    /// <summary>
    /// <para/>Executes provided function with retries in same thread.
    /// <para/>Throws last exception if exceeded <paramref name="maxAttempts"/>. Also see parameters description.
    /// </summary>
    /// <param name="func">Function to repeat. Throws if null</param>
    /// <param name="maxAttempts">Amount of attempts to repeat. Throws if less than 1</param>
    /// <param name="retryDelay">Amount of milliseconds to wait between retries</param>
    public static T ExecuteWithRetry<T>(Func<T> func, int maxAttempts = 3, int retryDelay = 0)
    {
      func.ThrowIfNull(nameof(func));
      if (maxAttempts < 1) { throw new ArgumentException(nameof(maxAttempts)); }

      Exception exception = null;
      T result = default(T);
      for (int i = 0; i < maxAttempts; i++)
      {
        if (Automation.TryCatch(() => result = func.Invoke(), out exception)) { return result; }
        Thread.Sleep(retryDelay);
      }
      throw exception;
    }
  }
}