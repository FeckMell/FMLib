using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Utils
{
  /// <summary>
  /// Base class that provides with <see cref="INotifyPropertyChanged"/> methods
  /// </summary>
  public abstract class NotifyPropertyChanged : INotifyPropertyChanged
  {
    /// <summary>
    /// Property changed event
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// On changed event for auto name fill
    /// </summary>
    protected void OnChanged([CallerMemberName]string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}