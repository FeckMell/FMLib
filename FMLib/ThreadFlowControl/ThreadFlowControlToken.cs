using System.Threading;

namespace Utils
{
  /// <summary>
  /// Token used to be passed to thread for it flow control.
  /// </summary>
  public class ThreadFlowControlToken
  {

    #region Helper classes

    /// <summary>
    /// Result of wait
    /// </summary>
    public enum WaitResult
    {
      /// <summary> Wait finished due to canceling </summary>
      Canceled,
      /// <summary> Wait finished normally </summary>
      NotCanceled
    }

    #endregion

    #region Fields

    /// <summary>
    /// Token that is shared across all tokens from parent <see cref="ThreadFlowControl"/> to be canceled all together
    /// </summary>
    private readonly ThreadFlowControlTokenShared m_sharedToken;

    /// <summary>
    /// Signal to cancel this token only
    /// </summary>
    private readonly ManualResetEvent m_cancelSignal;

    /// <summary>
    /// Is this child token
    /// </summary>
    private readonly bool m_isChild = false;

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

      if (m_isChild) { return; }
      CancelThis();
      m_cancelSignal.Dispose();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <remarks>MUST BE INTERNAL and can't be used outside from ThreadFlowControl!</remarks>
    internal ThreadFlowControlToken(ThreadFlowControlTokenShared shared)
    {
      m_sharedToken = shared;
      m_cancelSignal = new ManualResetEvent(false);
    }

    /// <summary>
    /// Constructor for child token
    /// </summary>
    private ThreadFlowControlToken(ThreadFlowControlToken threadFlowControlToken)
    {
      m_isChild = true;
      m_sharedToken = threadFlowControlToken.m_sharedToken;
      m_cancelSignal = threadFlowControlToken.m_cancelSignal;
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Creates child token from this one. The only difference from parent is that it is not disposed on <see cref="ThreadFlowControlToken.Dispose"/> called
    /// </summary>
    public ThreadFlowControlToken MakeChild()
    {
      return new ThreadFlowControlToken(this);
    }

    /// <summary>
    /// Waits for timer or to be canceled. Implemented in <see cref="ThreadFlowControlTokenShared"/>
    /// </summary>
    /// <returns> FALSE when canceled </returns>
    public WaitResult WaitForTimer(int time)
    {
      if (m_isDisposed) { return WaitResult.Canceled; }
      return (m_sharedToken.WaitForTimer(m_cancelSignal, time)) ? WaitResult.NotCanceled : WaitResult.Canceled;
    }

    /// <summary>
    /// Will block execution until canceled. Implemented in <see cref="ThreadFlowControlTokenShared"/>
    /// </summary>
    public void WaitToBeCanceled()
    {
      if (m_isDisposed) { return; }
      m_sharedToken.WaitToBeCanceled(m_cancelSignal);
    }

    /// <summary>
    /// Cancels thread this token was passed to
    /// </summary>
    public void CancelThis()
    {
      if (m_isDisposed) { return; }
      m_cancelSignal.Set();
    }

    #endregion

  }
}