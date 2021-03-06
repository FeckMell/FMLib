﻿using System;
using System.IO;

namespace Utils.Configuration
{
  /// <summary>
  /// Class containing information needed for correct application work
  /// </summary>
  public static class GlobalInformation
  {
    public const string CLIENT_DATABASE_FOLDER = "ClientDatabase";
    public const string CONFIGURATION_FOLDER = "Configuration";
    public const string LOGS_FOLDER = "Logs";
    public const string RUNTIME_FOLDER = "RunTime";
    public const string APP_TYPE_CLIENT = "Client";
    public const string APP_TYPE_SERVER = "Server";

    /// <summary>
    /// <see langword="static"/> constructor for initialisation.
    /// </summary>
    static GlobalInformation()
    {
      PathAppHome = AppDomain.CurrentDomain.BaseDirectory;
      Automation.TryCatch(
        () => AppVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(),
        (ex) => AppVersion = "Unknown build version");
      Automation.TryCatch(
        () => AppBuildTime = $"{new FileInfo(AppDomain.CurrentDomain.BaseDirectory + AppDomain.CurrentDomain.FriendlyName).LastWriteTimeUtc:O}",
        (ex) => AppBuildTime = "Unknown build time");
    }

    /// <summary>
    /// This application code
    /// </summary>
    public static string AppCode { get; set; } = "APP";

    /// <summary>
    /// Application type
    /// </summary>
    public static string AppType { get; set; } = APP_TYPE_CLIENT;

    /// <summary>
    /// Application build version
    /// </summary>
    public static string AppVersion { get; private set; }

    /// <summary>
    /// Application build time
    /// </summary>
    public static string AppBuildTime { get; private set; }

    /// <summary>
    /// Application home folder where all others are situated. Default: exe folder
    /// <para/> Setting that value will update all paths based on provided value. <see cref="UpdatePaths(string)"/>
    /// </summary>
    public static string PathAppHome { get => s_pathAppHome; set => UpdatePaths(value); }
    private static string s_pathAppHome = string.Empty;

    /// <summary>
    /// Path for Configuration files folder
    /// </summary>
    public static string PathConfiguration { get; set; } = string.Empty;

    /// <summary>
    /// Path for Log files folder
    /// </summary>
    public static string PathLog { get; set; } = string.Empty;

    /// <summary>
    /// Path for RunTime state save files folder
    /// </summary>
    public static string PathRuntime { get; set; } = string.Empty;

    /// <summary>
    /// Path for client databases files folder
    /// </summary>
    public static string PathClientDatabase { get; set; } = string.Empty;

    /// <summary>
    /// Format of Date and Time
    /// </summary>
    public static string DateTimeFormat { get; set; } = DateFormat + " " + TimeFormat;

    /// <summary>
    /// Format of Date
    /// </summary>
    public static string DateFormat { get; set; } = "yyyy.MM.dd";

    /// <summary>
    /// Format of Time
    /// </summary>
    public static string TimeFormat { get; set; } = "HH:mm:ss.fff";

    #region Private methods

    /// <summary>
    /// Updates all paths based on <paramref name="value"/>
    /// </summary>
    private static void UpdatePaths(string value)
    {
      s_pathAppHome = value;

      PathConfiguration = Path.Combine(value, CONFIGURATION_FOLDER);
      PathLog = Path.Combine(value, LOGS_FOLDER);
      PathRuntime = Path.Combine(value, RUNTIME_FOLDER);
      PathClientDatabase = Path.Combine(value, CLIENT_DATABASE_FOLDER);
    }

    #endregion

  }
}