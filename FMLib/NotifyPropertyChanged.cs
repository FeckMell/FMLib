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
    /// On changed event for single argument
    /// </summary>
    protected void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>
    /// On changed event for auto name fill
    /// </summary>
    protected void OnChangedAuto([CallerMemberName]string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}