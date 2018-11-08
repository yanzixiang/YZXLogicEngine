using System;
using System.Threading;

using Siemens.Simatic.PlcSim.VplcApi;

using Common;

namespace YZXLogicEngine
{
  public partial class YZXCPU
  {
    public string S7CPUPath { get; set; }
    public IPlc S7PLC;
    private WorkQueue PlcEvents;

    public static int S7VPLC_PIO_Size = 1024;

    #region Memory
    YZXCPUMemory S7VPLCIn;
    YZXCPUMemory S7VPLCOut;

    public void ConfigS7VPLCInput()
    {
      S7VPLCIn = new YZXCPUMemory("S7VPLCIn", S7VPLC_PIO_Size);
      Mermories["S7VPLCIn"] = S7VPLCIn;
    }
    public void ConfigS7VPLCOutput()
    {
      S7VPLCOut = new YZXCPUMemory("S7VPLCOut", S7VPLC_PIO_Size);
      Mermories["S7VPLCOut"] = S7VPLCOut;
    }
    #endregion Memory

    public void StartS7Plc(string path,PlcType plcFamily=PlcType.S71500)
    {
      WorkQueue plcEvents = new WorkQueue();
      plcEvents.Pause();
      EventWaitHandle eventWaitHandle = SpawnVplc(plcFamily, plcEvents, path);
      bool flag = eventWaitHandle.WaitOne(TimeSpan.FromMinutes(10.0));
    }

    internal EventWaitHandle SpawnVplc(PlcType plcType, WorkQueue plcEvents, string path)
    {
      this.PlcEvents = plcEvents;
      ManualResetEvent vplcIsReady = new ManualResetEvent(false);
      Thread thread = new Thread(() =>
      {
        try
        {
          this.S7PLC = new ClientPlc(plcType);
          this.SubscribeToVplcEvents();
          this.S7PLC.PowerOn(path);
        }
        catch (Exception ex)
        {
          if (this.S7PLC == null)
            return;
          this.S7PLC.PowerOff();
        }
        finally
        {
          vplcIsReady.Set();
        }
      })
      {
        Name = "Vplc Startup"
      };
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
      return vplcIsReady;
    }

    private void SubscribeToVplcEvents()
    {
      if (this.S7PLC == null)
        return;
      this.S7PLC.NetworkAddressChanged += new EventHandler<StringEventArgs>(this.UpdateIp);
      this.S7PLC.PowerOnInitiated += Plc_PowerOnInitiated;
      this.S7PLC.PowerOnCompleted += Plc_PowerOnCompleted;
      this.S7PLC.VplcExited += Plc_VplcExited;
      this.S7PLC.EndOfScan += Plc_EndOfScan;
      this.S7PLC.ConfigurationChanged += new EventHandler(this.OnPlcConfigurationChanged);
      this.S7PLC.NetworkAddressChanging += new EventHandler(this.OnDisconnectSignalFromPlc);
      this.S7PLC.HwConfigurationChanging += new EventHandler(this.OnDisconnectSignalFromPlc);
      this.S7PLC.SwConfigurationChanging += new EventHandler(this.OnDisconnectSignalFromPlc);
      this.S7PLC.FactoryResetInitiated += new EventHandler<BooleanEventArgs>(this.OnFactoryReset);
      this.S7PLC.MemoryResetInitiated += new EventHandler(this.OnDisconnectSignalFromPlc);
    }

    private void Plc_EndOfScan(object sender, EventArgs e)
    {
      Console.WriteLine($"Plc_EndOfScan");
      Console.WriteLine(DateTime.Now);
      byte[] piobs = S7PLC.ReadOutput(0, (ulong)S7VPLC_PIO_Size);

      
    }

    private void Plc_VplcExited(object sender, EventArgs e)
    {
      Console.WriteLine("Plc_VplcExited");
    }

    public const string S7VPLC_PowerOnCompleted = @"IronPython\S7VPLC.PowerOnCompleted.py";
    private void Plc_PowerOnCompleted(object sender, EventArgs e)
    {
      Console.WriteLine("Plc_PowerOnCompleted");
      if (FileSystem.FileExists(S7VPLC_PowerOnCompleted))
      {
        InitPY();
        Scope.SetVariable("PLC", this);
        Scope.SetVariable("CPU", S7PLC);
        RunPY(S7VPLC_PowerOnCompleted);
        S7PLC.Run();
      }
    }

    private void Plc_PowerOnInitiated(object sender, EventArgs e)
    {
      Console.WriteLine("Plc_PowerOnInitiated");
    }

    private void UpdateIp(object sender, StringEventArgs e)
    {
      //this.PlcEvents.Add(() => this.AddressUpdater.UpdateIpAddress(e.Value));
    }

    private void OnPlcConfigurationChanged(object sender, EventArgs e)
    {
      //this.PlcEvents.Add(() => this.SyncInvoke.Invoke(new Action(this.Connect), null));
    }

    private void OnDisconnectSignalFromPlc(object sender, EventArgs e)
    {
      this.PlcEvents.Pause();
      //this.OnMainThread(new Action(this.TryDisconnect));
      this.PlcEvents.Resume();
    }
    private void OnFactoryReset(object sender, BooleanEventArgs e)
    {
      if (!e.Value)
        this.UpdateIp(null, new StringEventArgs(null));
      this.PlcEvents.Pause();
      //this.OnMainThread(new Action(this.TryDisconnect));
      //this.OnMainThread(() => this.HardwareConfig.ResetConfiguration(IWorkingContextExtension.GetDlc<ICommandProcessor>(this.WorkingContext, (string)null)));
      this.PlcEvents.Resume();
    }
  }
}
