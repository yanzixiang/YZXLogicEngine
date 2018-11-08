using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Windows;

using Extensions;

using YZXMessaging;

using YZXLogicEngine.Unit;
using YZXLogicEngine.Task;

namespace YZXLogicEngine
{
  /// <summary>
  /// HMI中工程
  /// </summary>
  public class YZXCPUProject
  {

    public string CPUAddress{get;private set;}

    /// <summary>
    /// 内存区域
    /// </summary>
    public Dictionary<string, int> Mermories { get; private set; }

    public string S7CPUPath { get; private set; }

    /// <summary>
    /// S7Client
    /// </summary>
    public Dictionary<string, YZXS7Client> S7Clients { get; set; }

    /// <summary>
    /// 模块
    /// </summary>
    public List<Tuple<YZXUnitTypes, YZXUnit>> Units { get; private set; }

    public List<YZXUnitMemoryMap> Maps;

    /// <summary>
    /// 任务
    /// </summary>
    public List<Tuple<YZXTaskTypes,YZXTask>> Tasks { get; private set; }
    public YZXCPUProject()
    {
      Init();
    }

    public void Init()
    {
      Mermories = new Dictionary<string, int>();
      S7Clients = new Dictionary<string, YZXS7Client>();
      Units = new List<Tuple<YZXUnitTypes,YZXUnit>>();
      Maps = new List<YZXUnitMemoryMap>();
      Tasks = new List<Tuple<YZXTaskTypes, YZXTask>>();
    }

    public YZXCPUProject(string ProjectPath) :
      this()
    {
      LoadCPUProject(ProjectPath);
    }

    /// <summary>
    /// 从项目文件加载项目
    /// </summary>
    /// <param name="ProjectPath"></param>
    public void LoadCPUProject(string ProjectPath)
    {
      XElement root;
      try
      {
        root = XElement.Load(ProjectPath);

        IEnumerable<XElement> itemsXML;

        CPUAddress = root.Element("CPUAddress").Value;

        //内存区域
        itemsXML = root.Elements("Mermories").Elements("Mermory");
        foreach (XElement item in itemsXML)
        {
          string name = item.Element("Name").Value;
          int Length = item.Element("Length").Value.ToInt();
          Mermories[name] = Length;
        }

        S7CPUPath = root.Element("S7CPU").Element("Path").Value;

        //S7Clients
        itemsXML = root.Elements("S7Clients").Elements("S7Client");
        foreach (XElement item in itemsXML)
        {
          string name = item.Element("Name").Value;
          YZXS7Client client = 
            item.Element("XML").Value.XmlDeserializeFromString<YZXS7Client>();


          S7Clients[name] = client;
        }

        //模块
        itemsXML = root.Elements("Units").Elements("Unit");
        foreach (XElement item in itemsXML)
        {
          string xml = item.Element("XML").Value;
          switch (item.Element("Type").Value)
          {
            case "Random":
              RandomUnit ru = xml.XmlDeserializeFromString<RandomUnit>();
              Units.Add(new Tuple<YZXUnitTypes,YZXUnit>(YZXUnitTypes.Random,ru));
              break;
            case "ModbusTCP":
              ModbusTCPUnit mu = xml.XmlDeserializeFromString<ModbusTCPUnit>();
              Units.Add(new Tuple<YZXUnitTypes, YZXUnit>(YZXUnitTypes.ModbusTCP, mu));
              break;
            case "Snap7ClientDB":
              Snap7ClientDBUnit du = xml.XmlDeserializeFromString<Snap7ClientDBUnit>();
              Units.Add(new Tuple<YZXUnitTypes, YZXUnit>(YZXUnitTypes.Snap7ClientDB, du));
              break;
          }
        }

        //映射
        itemsXML = root.Elements("Maps").Elements("Map");
        foreach (XElement item in itemsXML)
        {
          string unit = item.Element("Unit").Value;
          string memory = item.Element("Memory").Value;
          ushort begin = item.Element("Begin").Value.ToUInt16();
          ushort length = item.Element("Length").Value.ToUInt16();

          YZXUnitMemoryMap map = new YZXUnitMemoryMap(unit,memory,begin,length);
          
          Maps.Add(map);
        }

        //任务
        itemsXML = root.Elements("Tasks").Elements("Task");
        foreach (XElement item in itemsXML)
        {
          string xml = item.Element("XML").Value;
          switch (item.Element("Type").Value)
          {
            case "IronPython":
              IronPythonTask ru = xml.XmlDeserializeFromString<IronPythonTask>();
              Tasks.Add(new Tuple<YZXTaskTypes, YZXTask>(YZXTaskTypes.IronPython, ru));
              break;
          }
        }



      }
      catch (Exception e)
      {
        MessageBox.Show(e.StackTrace, e.Message, 
          MessageBoxButton.OK, MessageBoxImage.Error);
      }
    }

    public void SaveDemoProject()
    {
      Mermories["DI"] = 100;
      Mermories["DO"] = 100;
      Mermories["M"] = 100;
      Mermories["R"] = 100;
      Mermories["DB1"] = 512;
      Mermories["MBT1"] = 100;

      RandomUnit RandomUnit1 = new RandomUnit("RandomUnit1", 100);

      RandomUnit1.Mins[0] = 0;
      RandomUnit1.Maxs[0] = 100;

      RandomUnit1.Mins[1] = 0;
      RandomUnit1.Maxs[1] = 100;

      Units.Add(new Tuple<YZXUnitTypes,YZXUnit>(YZXUnitTypes.Random,RandomUnit1));

      YZXS7Client s7client = new YZXS7Client("local","127.0.0.1");
      //s7client.con();

      Snap7ClientDBUnit db1 = new Snap7ClientDBUnit(s7client, 1, 512);
      Units.Add(new Tuple<YZXUnitTypes, YZXUnit>(YZXUnitTypes.Snap7ClientDB, db1));
      
      
      ModbusTCPUnit mbt1 = new ModbusTCPUnit("127.0.0.1", 502, 0, 100);

      Units.Add(new Tuple<YZXUnitTypes, YZXUnit>(YZXUnitTypes.ModbusTCP, mbt1));

      Maps.Add(new YZXUnitMemoryMap("RandomUnit1","R"));
      Maps.Add(new YZXUnitMemoryMap(mbt1.Name, "MBT1"));
      Maps.Add(new YZXUnitMemoryMap(db1.Name, "DB1",0,512));

      IronPythonTask ipytask = new IronPythonTask("Task1","Ironpython/Task1.py");
      ipytask.RunType = YZXTaskRunType.CONTINUE;
      Tasks.Add(new Tuple<YZXTaskTypes, YZXTask>(YZXTaskTypes.IronPython, ipytask));


      SaveCPUProject("DemoCPUProject.xml");
    }

    public void SaveCPUProject(string SavePath)
    {
      XElement project = new XElement("CPUProject");


      XElement cpuaddress = new XElement("CPUAddress", CPUAddress);
      project.Add(cpuaddress);

      //内存区域
      XElement ms = new XElement("Mermories");
      foreach (KeyValuePair<string, int> m in Mermories)
      {
        XElement item = new XElement("Mermory");

        XElement name = new XElement("Name", m.Key);
        XElement length = new XElement("Length", m.Value);
        item.Add(name);
        item.Add(length);

        ms.Add(item);
      }
      project.Add(ms);

      XElement s7cs = new XElement("S7Clients");
      foreach (KeyValuePair<string, YZXS7Client> s in S7Clients)
      {
        XElement item = new XElement("S7Client");

        XElement name = new XElement("Name", s.Key);
        XElement xml = new XElement("XML", s.Key.SerializeObject());

        item.Add(name);
        item.Add(xml);
      }
      project.Add(s7cs);

      //模块
      XElement units = new XElement("Units");
      foreach (Tuple<YZXUnitTypes, YZXUnit> u in Units)
      {
        XElement item = new XElement("Unit");

        XElement type = new XElement("Type", u.Item1.ToString());
        XElement xml = new XElement("XML", u.Item2.Serialize());
        item.Add(type);
        item.Add(xml);

        units.Add(item);
      }
      project.Add(units);

      //映射
      XElement maps = new XElement("Maps");
      foreach (YZXUnitMemoryMap map in Maps)
      {
        XElement item = new XElement("Map");

        XElement Unit = new XElement("Unit", map.Unit);
        XElement Memory = new XElement("Memory", map.Memory);
        XElement Start = new XElement("Begin", map.Begin);
        XElement Length = new XElement("Length", map.Length);

        item.Add(Unit);
        item.Add(Memory);
        item.Add(Start);
        item.Add(Length);

        maps.Add(item);
      }
      project.Add(maps);

      //任务
      XElement tasks = new XElement("Tasks");
      foreach (Tuple<YZXTaskTypes, YZXTask> u in Tasks)
      {
        XElement item = new XElement("Task");

        XElement type = new XElement("Type", u.Item1.ToString());
        XElement xml = new XElement("XML", u.Item2.Serialize());
        item.Add(type);
        item.Add(xml);

        tasks.Add(item);
      }
      project.Add(tasks);

      project.SaveFile();
    }
  }
}
