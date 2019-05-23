using System;
using System.Collections.Generic;
using System.Linq;
namespace FMLib.ThreadFlowControl
{
  /// <summary>
  /// Represents something like <see cref="CancellationToken"/> but extended with methods for Resume and Pause
  /// </summary>
  public class ThreadFlowControl
  {

    #region Fields

    /// <summary>
    /// Token that is shared among tokens created to have ability to cancel them all
    /// </summary>
    private ThreadFlowControlTokenShared m_token;

    #endregion

    #region Constructors and Destructors

    /// <summary>
    /// Determines if object was disposed
    /// </summary>
    private bool m_isDisposed = false;

    /// <summary>
    /// Dispose unmanaged resources
    /// </summary>
    public void Dispose()
    {
      if (m_isDisposed) { return; }
      m_isDisposed = true;

      Cancel();
      m_token.Dispose();
      //m_token = null;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public ThreadFlowControl()
    {
      m_token = new ThreadFlowControlTokenShared();
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Gets token that will help to control threads behavior
    /// </summary>
    /// <returns></returns>
    public ThreadFlowControlToken GetToken()
    {
      return new ThreadFlowControlToken(m_token ?? (m_token = new ThreadFlowControlTokenShared()));
    }

    /// <summary>
    /// Make all threads that have token from that source to wait until Resumed or Canceled
    /// </summary>
    public void Pause()
    {
      if (m_isDisposed) { return; }
      m_token.SetIsPaused(true);
      m_token.SetIsCanceled(false);
      m_token.SetCanceledSignalState(false);//true - unblock, false - block
      m_token.SetPausedSignalState(false);//true - unblock, false - block
    }

    /// <summary>
    /// Make all threads that have token from that source to Resume if were Paused
    /// </summary>
    public void Resume()
    {
      if (m_isDisposed) { return; }
      m_token.SetIsPaused(false);
      m_token.SetIsCanceled(false);
      m_token.SetCanceledSignalState(false);//true - unblock, false - block
      m_token.SetPausedSignalState(true);//true - unblock, false - block
    }

    /// <summary>
    /// Make all threads that have token from that source to Cancel (Finish their work)
    /// </summary>
    public void Cancel()
    {
      m_token.SetIsPaused(false);
      m_token.SetIsCanceled(true);
      m_token.SetCanceledSignalState(true);//true - unblock, false - block
      m_token.SetPausedSignalState(false);//true - unblock, false - block
    }

    #endregion

  }
}