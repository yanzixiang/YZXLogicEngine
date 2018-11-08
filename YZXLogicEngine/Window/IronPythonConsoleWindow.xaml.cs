using System;
using System.Collections.Generic;
using System.Windows;

namespace YZXLogicEngine
{
  public partial class IronPythonConsoleWindow : Window
  {
    public IronPythonConsoleWindow()
    {
      InitializeComponent();
      PCV.Loaded += PCV_Loaded;
    }

    private void PCV_Loaded(object sender, RoutedEventArgs e)
    {
      PCV.Pad.Host.ConsoleCreated += Host_ConsoleCreated;
    }

    private void Host_ConsoleCreated(object sender, EventArgs e)
    {
      PCV.Pad.Host.Console.ConsoleInitialized += Console_ConsoleInitialized;
    }
    private void Console_ConsoleInitialized(object sender, EventArgs e)
    {
      PCV.SetVariable("self", this);

      PCV.UpdateVariables();

      var console = PCV.Pad.Console;
      Dispatcher.BeginInvoke((Action)(() =>
      {
        PCV.Pad.Control.WordWrap = true;
        console.ExecuteFile(initFile);
        this.Activate();
      }));
    }

    private string initFile = "C:\\IronPython\\init.py";
    public string InitFile
    {
      get
      {
        return initFile;
      }
      set
      {
        initFile = value;
      }
    }
  }
}
