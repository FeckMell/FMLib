namespace System.Diagnostics
{
  /// <summary>
  /// Extensions to work with <see cref="Process"/>
  /// </summary>
  public static class ExtensionsProcess
  {
    /// <summary>
    /// Wraps <see cref="Process.Start()"/> for auto invoking pre- and post- Dispose actions when <paramref name="process"/> has exited.
    /// </summary>
    public static void StartSelfDisposingProcess(this Process process, Action preDisposeAction = null, Action postDisposeAction = null)
    {
      if (process == null) { return; }

      process.EnableRaisingEvents = true;
      process.Exited += (s, e) => { preDisposeAction?.Invoke(); process?.Dispose(); };
      process.Disposed += (s, e) => { postDisposeAction?.Invoke(); };
      //process.ErrorDataReceived += (s, e) => { Tracer.Get("External").Error($"External process {process?.StartInfo?.FileName} {caller} sent error: {e?.Data}"); };
      //process.OutputDataReceived += (s, e) => { Tracer.Get("External").Info($"External process {process?.StartInfo?.FileName} {caller} sent data: {e?.Data}"); };

      process.Start();
    }
  }
}