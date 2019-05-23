using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FMLib
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
    /// On changed event for many arguments
    /// </summary>
    /// <param name="names"></param>
    protected void OnChanged(string[] names)
    {
      if(names == null) { return; }
      foreach(var e in names)
      {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e));
      }
    }

    /// <summary>
    /// On changed event for single argument
    /// </summary>
    /// <param name="name"></param>
    protected void OnChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    /// <summary>
    /// On changed event for auto name fill
    /// </summary>
    /// <param name="name"></param>
    protected void OnChangedAuto([CallerMemberName]string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
  }
}