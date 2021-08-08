using System;
using System.Collections.Specialized;
using System.Management;
using System.Runtime.Serialization;
using System.Threading;
using BMCLV2.JsonClass;

namespace BMCLV2
{
  public class Report
  {
    public void RunBackGround()
    {
      var thread = new Thread(Run);
      thread.Start();
      thread.IsBackground = true;
    }

    public void Run()
    {
      var sysinfoJson = new JSON<SysInfoSchema>().Stringify(new SysInfoSchema());
      try
      {
        var data = new NameValueCollection
        {
          { "id", BmclCore.Config.Username },
          { "sysinfo", sysinfoJson },
          { "version", BmclCore.BmclVersion }
        };
        var webClient = new Downloader.Downloader();
        webClient.UploadValues("https://bmcl.bangbang93.com/usage", data);
      }
      catch (Exception ex)
      {
        Logger.Log(ex);
      }
    }
  }

  [DataContract]
  public class SysInfoSchema
  {
    [DataMember(Name = "memory")] public string Memory;
    [DataMember(Name = "cpu")] public string Cpu;
    [DataMember(Name = "bit")] public string Bit;
    [DataMember(Name = "video")] public string Video;
    [DataMember(Name = "system")] public string System;

    public SysInfoSchema()
    {
      var capacity = 0.0;
      var cimobject1 = new ManagementClass("Win32_PhysicalMemory");
      var moc1 = cimobject1.GetInstances();
      foreach (var o in moc1)
      {
        var mo1 = (ManagementObject)o;
        capacity += Math.Round(long.Parse(mo1.Properties["Capacity"].Value.ToString()) / 1024.0 / 1024.0, 1);
      }

      moc1.Dispose();
      cimobject1.Dispose();
      Memory = capacity.ToString("f0") + "MB";

      try //系统位数，系统名称
      {
        var searcher = new ManagementClass("WIN32_Processor");
        var moc = searcher.GetInstances();
        foreach (var o in moc)
        {
          var mo = (ManagementObject)o;
          Cpu = mo["Name"].ToString().Trim();
          Bit = mo["AddressWidth"].ToString().Trim() + "Bit";
        }
      }
      catch
      {
        // ignored
      }

      try //显卡， 支持多显卡
      {
        var searcher = new ManagementClass("Win32_VideoController");
        var moc = searcher.GetInstances();
        foreach (var mo in moc)
        {
          Video += mo["Name"].ToString().Trim() + "\n";
        }
      }
      catch
      {
        // ignored
      }

      System = Environment.OSVersion.VersionString;
    }
  }
}
