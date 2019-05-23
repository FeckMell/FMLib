using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FMLib.ThreadFlowControl
{
  /// <summary>
  /// Token for wait handle
  /// </summary>
  internal class ThreadFlowControlTokenShared
  {

    #region Fields

    /// <summary>
    /// Cancellation flag
    /// </summary>
    private bool m_isCanceled = false;

    /// <summary>
    /// Cancellation handle
    /// </summary>
    private readonly ManualResetEvent m_cancelSignal;

    /// <summary>
    /// Pause flag
    /// </summary>
    private bool m_isPaused = false;

    /// <summary>
    /// Pause handle
    /// </summary>
    private readonly ManualResetEvent m_pauseSignal;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Determines if object was disposed
    /// </summary>
    private bool m_isDisposed = false;
    private int m_lockCounter = 0;
    private ManualResetEvent m_disposeSignal = new ManualResetEvent(false);

    /// <summary>
    /// Dispose unmanaged resources
    /// </summary>
    internal void Dispose()
    {
      if (m_isDisposed) { return; }
      m_isDisposed = true;

      if (WaitHandle.WaitAny(new WaitHandle[] { m_disposeSignal }, 1000) == WaitHandle.WaitTimeout) { Tracer.Create().Warning($"Exited dispose lock due to timeout: {m_lockCounter}"); }
      m_disposeSignal.Dispose();
      m_cancelSignal.Dispose();
      m_pauseSignal.Dispose();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <remarks>MUST BE INTERNAL and can't be used outside from <see cref="ThreadFlowControl"/>!</remarks>
    internal ThreadFlowControlTokenShared()
    {
      m_isPaused = false;
      m_isCanceled = false;
      m_pauseSignal = new ManualResetEvent(true);
      m_cancelSignal = new ManualResetEvent(false);
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Sets isPaused field of this
    /// </summary>
    /// <param name="value"></param>
    /// <remarks>MUST BE INTERNAL and can't be used outside from <see cref="ThreadFlowControl"/>!</remarks>
    internal void SetIsPaused(bool value)
    {
      m_isPaused = value;
    }

    /// <summary>
    /// Sets isCanceled field of this
    /// </summary>
    /// <param name="value"></param>
    /// <remarks>MUST BE INTERNAL and can't be used outside from <see cref="ThreadFlowControl"/>!</remarks>
    internal void SetIsCanceled(bool value)
    {
      m_isCanceled = value;
    }

    /// <summary>
    /// Sets or resets SignalState of PauseSignal.
    /// true - set; false = reset
    /// </summary>
    /// <param name="isSet"></param>
    /// <remarks>MUST BE INTERNAL and can't be used outside from <see cref="ThreadFlowControl"/>!</remarks>
    internal void SetPausedSignalState(bool isSet)
    {
      bool result = (isSet) ? m_pauseSignal.Set() : m_pauseSignal.Reset();
    }

    /// <summary>
    /// Sets or resets SignalState of CancelSignal.
    /// true - set; false = reset
    /// </summary>
    /// <param name="isSet"></param>
    /// <remarks>MUST BE INTERNAL and can't be used outside from <see cref="ThreadFlowControl"/>!</remarks>
    internal void SetCanceledSignalState(bool isSet)
    {
      bool result = (isSet) ? m_cancelSignal.Set() : m_cancelSignal.Reset();
    }

    /// <summary>
    /// Waits for time to be passed. Or cancel is signaled
    /// </summary>
    /// <param name="time"></param>
    /// <returns>false when canceled</returns>
    /// <remarks>MUST BE INTERNAL and can't be used outside from <see cref="ThreadFlowControl"/>!</remarks>
    internal bool WaitForTimer(WaitHandle personalCancellation, int time)
    {
      int waitResult = -1;
      //wait for time or cancellation signals
      if (time > 0)
      {
        LockDispose();
        waitResult = WaitHandle.WaitAny(new WaitHandle[] { m_cancelSignal, personalCancellation }, time);
        ReleaseDispose();
      }
      switch (waitResult)
      {
        case WaitHandle.WaitTimeout: break;
        case 0: return false;
        case 1: return false;
        default: break;
      }

      //If paused - wait when signaled to resume or to cancel
      if (m_isPaused)
      {
        LockDispose();
        waitResult = WaitHandle.WaitAny(new WaitHandle[] { m_cancelSignal, personalCancellation, m_pauseSignal });
        ReleaseDispose();
        switch (waitResult)
        {
          case 0: return false;
          case 1: return false;
          case 2: break;
        }
      }

      //If for some reason cancellation didn't work - check again
      return (m_isCanceled) ? false : true;
    }

    /// <summary>
    /// Waits to be canceled
    /// </summary>
    /// <param name="personalCancellation"></param>
    /// <remarks>MUST BE INTERNAL and can't be used outside from <see cref="ThreadFlowControl"/>!</remarks>
    internal void WaitToBeCanceled(WaitHandle personalCancellation)
    {
      LockDispose();
      WaitHandle.WaitAny(new WaitHandle[] { m_cancelSignal, personalCancellation });
      ReleaseDispose();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Locks dispose method
    /// </summary>
    private void LockDispose()
    {
      m_lockCounter++;
      if (m_lockCounter > 0) { m_disposeSignal.Reset(); }
    }

    /// <summary>
    /// Releases lock from dispose method
    /// </summary>
    private void ReleaseDispose()
    {
      m_lockCounter--;
      if (m_lockCounter == 0) { m_disposeSignal.Set(); }
      else if (m_lockCounter < 0) { Tracer.Create().Info($"lock counter is below zero: {m_lockCounter}"); m_disposeSignal.Set(); }
    }
    #endregion

  }
}