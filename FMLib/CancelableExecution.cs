using System;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
  /// <summary>
  /// Class that allow to run execution of methods that can be canceled. TODO: current doesn't work
  /// </summary>
  public class CancelableExecution
  {

    #region Helper classes
    /// <summary>
    /// Class representing cancel protector for CancelableExecution
    /// </summary>
    public class CancelProtector : IDisposable
    {
      /// <summary>
      /// Controls depth of protection
      /// </summary>
      private int m_protection = 0;

      /// <summary>
      /// Is protection enabled
      /// </summary>
      public bool IsProtected => m_protection > 0;

      /// <summary>
      /// Protects from canceling
      /// </summary>
      public IDisposable Protect()
      {
        m_protection++;
        return this;
      }

      /// <summary>
      /// Disposing. Also takes CancelLock
      /// </summary>
      public void Dispose() => m_protection = m_protection > 0 ? m_protection - 1 : m_protection;
    }
    #endregion

    #region Fields

    /// <summary>
    /// If can start
    /// </summary>
    public bool IsStartable { get; private set; } = true;

    /// <summary>
    /// If was canceled
    /// </summary>
    public bool IsCanceled { get; private set; } = false;

    /// <summary>
    /// If has finished before canceling
    /// </summary>
    public bool IsFinished { get; private set; } = false;

    private CancellationTokenSource m_tokenSource;
    private CancellationToken m_token;
    private Action m_action;
    private Action m_onCancel;
    private Action m_onFinished;
    private CancelProtector m_cancelProtector = new CancelProtector();

    #endregion

    #region Constructors

    /// <summary>
    /// Creates instance of CancelableExecution and starts execution.
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="onCancel">Action to perform on cancel</param>
    /// <param name="onFinished">Action to perform on finish</param>
    /// <returns>CancelableExecution instance that was started</returns>
    public static CancelableExecution StartNew(Action action, Action onCancel = null, Action onFinished = null)
    {
      var result = new CancelableExecution(action, onCancel, onFinished);
      result.Start();
      return result;
    }

    /// <summary>
    /// Constructor. Need to call <see cref="Start"/> to start execution
    /// </summary>
    /// <param name="action">Action to execute</param>
    /// <param name="onCancel">Action to perform on cancel</param>
    /// <param name="onFinished">Action to perform on finish</param>
    public CancelableExecution(Action action, Action onCancel = null, Action onFinished = null)
    {
      m_tokenSource = new CancellationTokenSource();
      m_token = m_tokenSource.Token;
      m_action = action;
      m_onCancel = onCancel;
      m_onFinished = onFinished;

      m_token.Register(Cancelation); // register exception to finish immediately
    }

    #endregion

    #region Public methods

    /// <summary>
    /// Starts execution of provided action
    /// </summary>
    public void Start()
    {
      if (!IsStartable)
        return;

      IsStartable = false;
      Task.Factory.StartNew(StartExecution, m_token).Wait();
    }

    /// <summary>
    /// Sets object in cancel protection state. Returns IDisposable for "using" pattern
    /// </summary>
    /// <returns></returns>
    public IDisposable CancelProtection()
    {
      return m_cancelProtector.Protect();
    }

    /// <summary>
    /// Cancel
    /// </summary>
    public void Cancel(bool? isForced = null)
    {
      if (IsStartable || IsCanceled || IsFinished)
        return; // if in that flags - can't cancel anything

      if (isForced.GetValueOrDefault() || !m_cancelProtector.IsProtected)
        CancelExecution();
      else
        DelayedCancel();
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Cancellation action
    /// </summary>
    private static void Cancelation() => throw new Exception("Token canceled");

    /// <summary>
    /// Starts execution
    /// </summary>
    private void StartExecution()
    {
      m_action?.Invoke(); // perform action
      IsFinished = true; // when finished Flag that
      m_onFinished?.Invoke(); // invoke onFinished action
      m_tokenSource?.Dispose(); // dispose token source because it is not needed anymore
      m_tokenSource = null; // set it to null
    }

    /// <summary>
    /// Cancels execution
    /// </summary>
    private void CancelExecution()
    {
      try { m_tokenSource?.Cancel(); } // calls cancel that will throw from execution thread
      catch { } // don't need to do anything here
      IsCanceled = true; // setting state to canceled
      m_tokenSource?.Dispose(); // dispose token source because it is not needed anymore
      m_tokenSource = null; // set it to null
      m_onCancel?.Invoke(); // invoke onCancel action
    }

    /// <summary>
    /// Tries to cancel current execution every 100ms.
    /// </summary>
    private void DelayedCancel()
    {
      Task.Factory.StartNew(() =>
      {
        while (!IsCanceled)
        {
          Task.Delay(100).Wait();
          if (!m_cancelProtector.IsProtected)
          {
            CancelExecution();
            break;
          }
        }
      });
    }

    #endregion

  }
}