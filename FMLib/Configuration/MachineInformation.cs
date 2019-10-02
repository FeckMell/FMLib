using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using FMLib.ExtensionMethods;
using Utils;
using Utils.Logging;

namespace FMLib.Configuration
{
  public static class MachineInformation
  {

    /// <summary>
    /// Static constructor
    /// </summary>
    static MachineInformation()
    {
      Automation.TryCatch(() => MachineName = Environment.MachineName, (ex) => MachineName = ex.Message);
      Automation.TryCatch(() => UserName = $"{Environment.UserDomainName}\\{Environment.UserName.Replace("'", string.Empty)}", (ex) => UserName = ex.Message);
    }

    /// <summary>
    /// Gets the name of the local machine.
    /// </summary>
    public static string MachineName { get; private set; }

    /// <summary>
    /// Gets the name of the user accessing the local machine.
    /// </summary>
    public static string UserName { get; private set; }

    /// <summary>
    /// Filter for IpAddress in case if have few network interfaces
    /// </summary>
    public static string PrimaryIPAddressFilter
    {
      get => s_primaryIPAddressFilter;
      set
      {
        s_primaryIPAddressFilter = value;
        s_ipAddress = null;
      }
    }
    private static string s_primaryIPAddressFilter;

    /// <summary>
    /// Gets the IP Address of the machine.
    /// Extended version of IpAddress to have ability to obtain correct address if PrimaryIPAddressFilter is given.
    /// </summary>
    public static string IpAddress
    {
      get
      {
        string ipResolvingError = "Error resolving IP address";
        s_ipAddress = s_ipAddress ?? ipResolvingError;

        if (s_ipAddress.EQUAL(ipResolvingError))
          s_ipAddress = (string.IsNullOrWhiteSpace(PrimaryIPAddressFilter) ? GetIP() : GetIPByFilter()) ?? s_ipAddress;

        return s_ipAddress;
      }
    }
    private static string s_ipAddress;

    /// <summary>
    /// Free space on drive with data <see cref="GlobalInformation.PathAppHome"/> in bytes
    /// </summary>
    public static double FreeDataDiskSpace
    {
      get
      {
        double result = -1;
        Automation.TryCatch(() => result = GetDriveForPath(GlobalInformation.PathAppHome).AvailableFreeSpace, (ex) => Tracer._Error(ex.FullInfo()));
        return result;
      }
    }

    /// <summary>
    /// Free space on drive with data <see cref="GlobalInformation.PathAppHome"/> in bytes
    /// </summary>
    public static double TotalDataDiskSpace
    {
      get
      {
        double result = -1;
        Automation.TryCatch(() => result = GetDriveForPath(GlobalInformation.PathAppHome).TotalSize, (ex) => Tracer._Error(ex.FullInfo()));
        return result;
      }
    }

    #region Private methods

    /// <summary>
    /// Gets drive by path. If fails - throws
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private static DriveInfo GetDriveForPath(string path)
    {
      var drive = DriveInfo.GetDrives().FirstOrDefault(x => x.Name.EQUAL(Path.GetPathRoot(path)));
      return drive ?? throw new Exception($"Couldn't find drive for path:{path}");
    }

    /// <summary>
    /// Gets machine IP address by classic algorithm (faster then by network interface)
    /// </summary>
    private static string GetIP()
    {
      try
      {
        IPAddress[] addresses = Dns.GetHostAddresses(MachineName);
        if (addresses != null)
        {
          foreach (IPAddress address in addresses)
            if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
              return address.ToString();
            }
        }
      }
      catch (Exception ex) { Tracer._Error(ex.ToString()); }
      return null;
    }

    /// <summary>
    /// Gets IP by PrimaryIPAddressFilter from various network interfaces.
    /// Call of NetworkInterface.GetAllNetworkInterfaces() is very expensive: takes about 130ms on TREK-773
    /// </summary>
    /// <returns></returns>
    private static string GetIPByFilter()
    {
      try
      {
        string regexFilter = "^" + Regex.Escape(PrimaryIPAddressFilter).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
        foreach (var network in networkInterfaces)
        {
          var properties = network.GetIPProperties();
          if (properties.GatewayAddresses.Count == 0)
            continue;

          foreach (var address in properties.UnicastAddresses)
          {
            if (address.Address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
              continue;

            if (IPAddress.IsLoopback(address.Address))
              continue;

            if (Regex.IsMatch(address.Address.ToString(), regexFilter))
              return address.Address.ToString();
          }
        }
      }
      catch (Exception ex) { Tracer._Error(ex.ToString()); }
      return null;
    }

    #endregion

  }
}