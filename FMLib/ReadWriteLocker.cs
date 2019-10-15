using System;
using System.Threading;

namespace Utils
{
  /// <summary>
  /// A read locker class to allow us to use IDisposable within "using" statement.
  /// Class wraps this famous Evgeny pattern.
  /// </summary>
  /// <remarks>
  /// The class is suitable mostly to synchronize a shared resource for often read operations, and rare write operations
  /// The class provides three levels of locking: Read, UpgradeableRead and Write
  /// The Read lock:
  ///  - is needed to synchronize read operations on shared resource
  ///  - doesn't stop unless another write lock is in progress
  ///  - can be called recursively inside any other lock type.
  ///
  /// The UpgradeableRead lock:
  ///  - is needed to start synchronize long read operations with further short writing operations, locked by write lock recursively
  ///  - competes with all other UpgradeableRead locks and write locks, but passes read locks
  ///  - can be called recursively inside UpgradeableRead and Write locks, calling inside the Read lock will cause an exception
  ///
  /// The Write lock:
  ///  - is needed to synchronize write operations on shared resource, the only type of locking which should be used for modifying shared resource
  ///  - competes with all locks for the shared resource
  ///  - can be called inside UpgradeableRead and Write locks, calling inside the Read lock will cause an exception
  /// </remarks>
  public class ReadWriteLocker
  {
    /// <summary>
    /// Delegate to log locker messages information
    /// </summary>
    /// <param name="message">The internal service message, can be null</param>
    public delegate void TraceDelegate(string message);

    /// <summary>
    /// The global tracing method, which is used when null tracing delegate is provided to the class.
    /// Can be globally set up.
    /// </summary>
    public static TraceDelegate GlobalTracingMethod = null;

    /// <summary>
    /// Create a read locker object to use with using keyword
    /// </summary>
    /// <returns>A read locker</returns>
    public IDisposable Read => new DisposableLocker(m_locker, DisposableLocker.LockType.Read);

    /// <summary>
    /// Create a write locker object to use with using keyword
    /// </summary>
    /// <returns>A write locker</returns>
    public IDisposable UpgradeableRead => new DisposableLocker(m_locker, DisposableLocker.LockType.UpgradeableRead);

    /// <summary>
    /// Create a write locker object to use with using keyword
    /// </summary>
    /// <returns>A write locker</returns>
    public IDisposable Write => new DisposableLocker(m_locker, DisposableLocker.LockType.Write);


    /// <summary>
    /// Construction
    /// </summary>
    public ReadWriteLocker()
    {
      m_locker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    }

    #region implementation

    // Never use directly
    private readonly ReaderWriterLockSlim m_locker;


    /// <summary>
    /// Encapsulate the m_locker and allow EnterRead/WriteLock and ExitRead/WriteLock using a "using" statement
    /// </summary>
    private class DisposableLocker : IDisposable
    {
      public enum LockType
      {
        Read,
        UpgradeableRead,
        Write,
      }

      // lock timeout warning condition
      private const int MaxLockSeconds = 1;
      // lock hanging error condition
      private const int HangingLockSeconds = 60;
      // lock duration warning condition
      private const int MaxExecuteSeconds = 2;


      private readonly LockType m_type;
      private readonly DateTime m_startTime;
      private ReaderWriterLockSlim m_locker;

      /// <summary>
      /// Construction
      /// </summary>
      /// <param name="locker">Shared lock</param>
      /// <param name="type">The type of synchronization</param>
      internal DisposableLocker(ReaderWriterLockSlim locker, LockType type)
      {
        m_type = type;
        m_startTime = DateTime.Now;
        m_locker = locker;

        bool entered = false;

        if (m_type == LockType.Read)
          entered = m_locker.TryEnterReadLock(TimeSpan.FromSeconds(HangingLockSeconds));
        else if (m_type == LockType.Write)
          entered = m_locker.TryEnterWriteLock(TimeSpan.FromSeconds(HangingLockSeconds));
        else if (m_type == LockType.UpgradeableRead)
          entered = m_locker.TryEnterUpgradeableReadLock(TimeSpan.FromSeconds(HangingLockSeconds));

        if (!entered)
        {
          ReadWriteLocker.GlobalTracingMethod?.Invoke($"Enter{m_type}Lock seems to be hanging for a {(DateTime.Now - m_startTime).TotalSeconds:F2} seconds already");

          if (m_type == LockType.Read)
            m_locker.EnterReadLock();
          else if (m_type == LockType.Write)
            m_locker.EnterWriteLock();
          else if (m_type == LockType.UpgradeableRead)
            m_locker.EnterUpgradeableReadLock();
        }

        if (DateTime.Now - m_startTime > TimeSpan.FromSeconds(MaxLockSeconds))
          ReadWriteLocker.GlobalTracingMethod?.Invoke($"Enter{m_type}Lock took {(DateTime.Now - m_startTime).TotalSeconds:F2} seconds");

        m_startTime = DateTime.Now;
      }

      /// <summary>
      /// Disposal use to exit the lock
      /// </summary>
      public void Dispose()
      {
        if (m_locker != null)
        {
          try
          {
            if (DateTime.Now - m_startTime > TimeSpan.FromSeconds(MaxExecuteSeconds))
              ReadWriteLocker.GlobalTracingMethod?.Invoke($"{m_type}Lock was held for {(DateTime.Now - m_startTime).TotalSeconds:F2} seconds");
          }
          finally
          {
            if (m_type == LockType.Read)
              m_locker.ExitReadLock();
            else if (m_type == LockType.Write)
              m_locker.ExitWriteLock();
            else if (m_type == LockType.UpgradeableRead)
              m_locker.ExitUpgradeableReadLock();

            m_locker = null;
          }
        }
      }
    }

    #endregion implementation
  }
}