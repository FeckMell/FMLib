namespace Utils
{
  /// <summary>
  /// Class gives controls on thread execution with methods: Cancel, Pause, Resume, Wait
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
    private Flag m_isDisposed;

    /// <summary>
    /// Dispose unmanaged resources
    /// </summary>
    public void Dispose()
    {
      if (m_isDisposed.CheckThenSet()) { return; }

      Cancel();
      m_token.Dispose();
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
    public ThreadFlowControlToken GetToken()
    {
      return new ThreadFlowControlToken(m_token);
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