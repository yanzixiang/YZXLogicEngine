using System;
using System.Collections.Generic;
using System.Threading;

using Common;
using Extensions;

using YZXMessaging;
using YZXLogicEngine;
using YZXLogicEngine.Task;
using YZXLogicEngine.Unit;
using YZXLogicEngine.UDT;

namespace YZXLogicEngine
{
  /// <summary>
  /// CPU
  /// 针对设备编程
  /// </summary>
  public partial class YZXCPU
  {
    public YZXCPUServer Server;

    /// <summary>
    /// 内存区域
    /// </summary>
    public Dictionary<string, YZXCPUMemory> Mermories { get; set; }

    public YZXCPUMemory CPUStatus;
    public YZXCPUMemory UnitStatus;
    public YZXCPUMemory TaskStatus;

    List<YZXS7Client> S7Clients;
    Dictionary<string, YZXUnit> Units;

    public YZXCPU()
    {
      Init();
    }

    public void Init() {
      InitPY();
      Mermories = new Dictionary<string, YZXCPUMemory>();

      UnitStatus = new YZXCPUMemory();

      Tasks = new Dictionary<string, YZXTask>();
      Timers = new Dictionary<string, Timer>();

      //HMIs = new Dictionary<string, YZXHMIClient>();
    }

    /// <summary>
    /// 启动
    /// </summary>
    public void Start()
    {
      //if (FileSystem.DirectoryExists(S7CPUPath))
      //{
      //  StartS7Plc(S7CPUPath);
      //  StartS7VPLCISOServer("192.168.8.100");
      //}

      StartTasks();
    }

    /// <summary>
    /// 停止
    /// </summary>
    public void Stop()
    {
      StopTasks();
    }

    public void Run() { }

    /// <summary>
    /// 映射模块到内存区域
    /// 同时添加读任务
    /// </summary>
    /// <param name="unit">模块</param>
    /// <param name="mermory">内存区域</param>
    /// <param name="begin">起始位置</param>
    /// <param name="end">结束位置</param>
    public void MapReadUnit(
      YZXUnit unit,
      YZXCPUMemory mermory,
      ushort begin,
      ushort length)
    {
      int end = begin + length - 1;
      mermory.Map(begin, length, unit);

      Action init = (() => { });
      Func<bool> run = (() =>
      {
        try
        {
          //从模块内读取所有数据
          List<bool> bits = unit.UpdateBits();

          //将读取到的数据更新到内存区域
          mermory.UpdateBits(begin, end, bits);

          return true;
        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
          return false;
        }
      });

      Action reset = (() => { });

      CSharpTask task = new CSharpTask(init, run, reset);

      task.Name = string.Format("{0}-[{1},{2}]-{3}",
        mermory.Name, begin, end, unit.Name);

      Tasks[task.Name] = task;
    }

    /// <summary>
    /// 按内存区域类型
    /// 映射模块到不同内存区域
    /// 同时添加读任务
    /// </summary>
    /// <param name="unit">模块</param>
    /// <param name="mermory">内存区域</param>
    /// <param name="begin">起始位置</param>
    /// <param name="end">结束位置</param>
    public void MapUnit(
        YZXUnit unit,
        YZXCPUMemory mermory,
        ushort begin,
        ushort length)
    {
      switch (mermory.MermoryType)
      {
        case MermoryType.Read:
          MapReadUnit(unit, mermory, begin, length);
          break;
        case MermoryType.Write:
          break;
        case MermoryType.S7VPLCInput:
          break;
        case MermoryType.S7VPLCOutput:
          break;
      }
      
    }


    #region 工程
    public void LoadProject(YZXCPUProject project)
    {
      try
      {
        //S7CPUPath = project.S7CPUPath;
        ConfigCPUMermories(project);
        ConfigCPUUnits(project);
        ConfigUnitMemoryMaps(project);
        ConfigTasks(project);
        //ServerIP = YZXCMessagingNode.ResolveServerIP();
        //ServerIP = project.CPUAddress;

        //StartCPUServer();
        //ConfigCPUServer();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex);
      }
    }

    /// <summary>
    /// 配置CPU内存区域
    /// </summary>
    public void ConfigCPUMermories(YZXCPUProject Project)
    {
      foreach (KeyValuePair<string, int> item in Project.Mermories)
      {
        Mermories[item.Key] = new YZXCPUMemory(item.Key, item.Value);
      }

      int statusL = YZXUDT.GetUDTLength(YZXUDTTypes.CPUStatus);
      CPUStatus = new YZXCPUMemory("CPUStatus", statusL);
      Mermories["CPUStatus"] = CPUStatus;
    }

    /// <summary>
    /// 配置CPU模块
    /// </summary>
    public void ConfigCPUUnits(YZXCPUProject Project)
    {

      int statusL = YZXUDT.GetUDTLength(YZXUDTTypes.UnitStatus);

      UnitStatus = new YZXCPUMemory("UnitStatus", Project.Units.Count * statusL);
      Mermories["UnitStatus"] = UnitStatus;

      Units = new Dictionary<string, YZXUnit>();

      for (int i = 0; i < Project.Units.Count; i++)
      {
        System.Tuple<YZXUnitTypes, YZXUnit> tu = Project.Units[i];

        YZXUnit unit = tu.Item2;
        if (tu.Item1 == YZXUnitTypes.Snap7ClientDB)
        {
          Snap7ClientDBUnit u = unit as Snap7ClientDBUnit;
          u.Client.con();
        }
        unit.Init();

        YZXUnitStatus status = (YZXUnitStatus)
          UnitStatus.InitUDT(YZXUDTTypes.UnitStatus,this,(i * statusL).ToString());

        status.Unit = unit;
        
        unit.Status = status;

        Units[unit.Name] = unit;
      }
    }

    public void ConfigUnitMemoryMaps(YZXCPUProject Project)
    {
      foreach (YZXUnitMemoryMap map in Project.Maps)
      {
        YZXUnit unit = Units[map.Unit];
        YZXCPUMemory memory = Mermories[map.Memory];
        ushort begin = map.Begin;
        ushort length = map.Length;
        MapUnit(unit, memory, begin, length);
      }
    }

    /// <summary>
    /// 配置任务
    /// </summary>
    public void ConfigTasks(YZXCPUProject Project)
    {
      AddGCTask();

      foreach (System.Tuple<YZXTaskTypes, YZXTask> tt in Project.Tasks)
      {
        YZXTask task = tt.Item2;
        if (tt.Item1 == YZXTaskTypes.IronPython)
        {
          IronPythonTask ipytask = task as IronPythonTask;
          ipytask.InitR();
          ConfigIronPythonTask(ipytask);
        }

        Tasks[task.Name] = task;
      }
    }

    public void ConfigIronPythonTask(IronPythonTask ipytask)
    {
      foreach (KeyValuePair<string, YZXCPUMemory> m in Mermories)
      {
        string name = m.Key;
        YZXCPUMemory memory = m.Value;
        ipytask.Engine.Runtime.Globals.SetVariable(name, memory);
      }
    }

    #endregion 工程
  }
}