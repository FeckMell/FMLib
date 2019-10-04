using System.Collections.Generic;

namespace Utils.ReferenceData
{
  /// <summary>
  /// Class representing data reference store
  /// </summary>
  public interface IReferenceDataStore
  {

    #region Active lists

    /// <summary>
    /// Gets all active lists
    /// </summary>
    List<string> GetAllActiveLists();

    /// <summary>
    /// Gets active list content by name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    List<string> GetActiveListContent(string name);

    #endregion

    /// <summary>
    /// Gets all groups of first level
    /// </summary>
    /// <returns></returns>
    List<IGroup> GetAllGroups();
  }
}