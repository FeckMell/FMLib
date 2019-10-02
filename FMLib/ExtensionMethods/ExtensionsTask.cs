namespace System.Threading.Tasks
{
  /// <summary>
  /// Extensions to work with <see cref="Task"/>
  /// </summary>
  public static class ExtensionsTask
  {
    /// <summary>
    /// <para/>Wraps <see cref="Task.Start()"/> for auto invoking pre- and post- Dispose actions.
    /// <para/>It invokes <paramref name="preDisposeAction"/> when task is completed, then calls <see cref="Task.Dispose()"/> and then invokes <paramref name="postDisposeAction"/>.
    /// </summary>
    public static void StartSelfDisposingTask(this Task task, Action preDisposeAction = null, Action postDisposeAction = null)
    {
      if (task == null) { return; }

      task
        .ContinueWith((t) => { preDisposeAction?.Invoke(); task?.Dispose(); })
        .ContinueWith((t) => { postDisposeAction?.Invoke(); });
      task.Start();
    }
  }
}