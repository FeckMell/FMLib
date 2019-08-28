using System;

namespace Utils
{


  /// <summary>
  /// Event arguments for port classes
  /// </summary>
  public class EventArgsPort : EventArgs
  {
    /// <summary>
    /// Specifies what type of data is stored in <see cref="EventArgsPort"/>
    /// </summary>
    public enum TypesPortEvent
    {
      /// <summary>Event is raised with data received</summary>
      MessageReceived = 0,
      /// <summary>Event is raised with error on reading</summary>
      ErrorOccured = 1,
    }

    /// <summary>
    /// Type of message
    /// </summary>
    public TypesPortEvent Type;

    /// <summary>
    /// Message received
    /// </summary>
    public NetworkData Data;

    /// <summary>
    /// ToString override
    /// </summary>
    public override string ToString()
    {
      return $"{{Type:{Type}, Data={Data}}}";
    }
  }
}