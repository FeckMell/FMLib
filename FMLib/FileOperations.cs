using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utils.Collections;
using Utils.ExtensionMethods;

namespace Utils
{
  /// <summary>
  /// Class contains common file operations for Simulator
  /// </summary>
  public static class FileOperations
  {
    /// <summary>
    /// Checks if file exists
    /// </summary>
    public static bool Exists(string filepath, DataTree<string> error)
    {
      try
      {
        Path.GetFullPath(filepath);
        if (filepath.IsNullOrWhiteSpace()) { throw new ExpectedException($"Filename argument is null({filepath == null}) or empty({filepath == null})"); }
        if (!File.Exists(filepath)) { throw new ExpectedException($"File {filepath} doesn't exist"); }
        return true;
      }
      catch (Exception ex)
      {
        error.Add(ex.Message);
        return false;
      }
    }

    /// <summary>
    /// Reads all test from file in <see cref="List{T}"/> where T is <see cref="string"/>
    /// </summary>
    public static bool ReadFileLineArray(string filepath, out List<string> file, DataTree<string> error)
    {
      try
      {
        if (!Exists(filepath, error)) { file = new List<string>(); return false; }
        file = File.ReadAllLines(filepath).ToList();
        return true;
      }
      catch (Exception ex)
      {
        error.Add($"Exception reading file {filepath}: {ex.Message}");
        file = new List<string>();
        return false;
      }
    }

    /// <summary>
    /// Reads all text from file
    /// </summary>
    public static bool ReadAllFile(string filepath, out string result, DataTree<string> error)
    {
      try
      {
        DataTree<string> subError = new DataTree<string>();
        if (!Exists(filepath, error)) { result = string.Empty; return false; }
        result = File.ReadAllText(filepath);
        return true;
      }
      catch (Exception ex)
      {
        error.Add($"Exception reading file {filepath}: {ex.Message}");
        result = string.Empty;
        return false;
      }
    }

    /// <summary>
    /// Creates directory recursive. Path with filename can be passed if file has extension.
    /// </summary>
    public static bool CreateDirectory(string path, DataTree<string> error)
    {
      if (path.IsNullOrWhiteSpace()) { error.Add("Path is empty."); return false; }
      try
      {
        if (!Path.GetExtension(path).IsNullOrEmpty()) { path = Path.GetDirectoryName(path); }
        if (Directory.Exists(path)) { return true; }
        Directory.CreateDirectory(path);
        return true;
      }
      catch (Exception ex) { error.Add($"Exception creating directory {path}: {ex.Message}"); return false; }
    }
  }
}