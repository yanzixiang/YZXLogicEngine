using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Reflection;

using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Exceptions;

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;

using Common;

using YZXLogicEngine.Task;

namespace YZXLogicEngine
{
  public partial class IronPythonDebuger : UserControl
  {
    public IronPythonTask Task;
    TextEditor avalonEdit;
    public string CurrentShowingFile;

    public List<string> AllowedFiles;
    public string CurrentFile { get; private set; }

    public bool Watching{get;set;}
    public IronPythonDebuger()
    {
      avalonEdit = new TextEditor();
      avalonEdit.ShowLineNumbers = true;
      avalonEdit.FontFamily = new FontFamily("Consolas");
      avalonEdit.FontSize = 14;
      avalonEdit.WordWrap = true;

      InitializeComponent();
      grid.Children.Add(avalonEdit);

      Loaded += IronPythonDebuger_Loaded;
    }

    private void IronPythonDebuger_Loaded(object sender, RoutedEventArgs e)
    {
      InitHighLight();
    }

    public void HighlightLine(int linenum, Brush foreground, Brush background)
    {
      //Console.WriteLine(String.Format("HighlightLine : {0}", linenum));
      

      if (Watching)
      {
        Dispatcher.Invoke((Action)(() =>
        {
          if (linenum > avalonEdit.Document.LineCount)
          {
            return;
          }
          try {
            avalonEdit.ScrollToLine(linenum);
            DocumentLine line = avalonEdit.Document.GetLineByNumber(linenum);
            avalonEdit.Select(line.Offset, line.TotalLength - 1);
            line = null;
          }catch(Exception ex)
          {
            Console.WriteLine(ex.ToString());
            ExceptionViewer ev = new ExceptionViewer("TracebackEvent", ex);
            ev.ShowDialog();
          }
        }));
      }
      if (Task != null)
      {
        Task.Resume();
      }
    }

    public void InitHighLight()
    {
      IHighlightingDefinition pythonHighlighting;
      using (Stream s = typeof(IronPythonDebuger).Assembly.GetManifestResourceStream("YZXLogicEngine.Resources.Python.xshd"))
      {
        if (s == null)
          throw new InvalidOperationException("Could not find embedded resource");
        using (XmlReader reader = new XmlTextReader(s))
        {
          pythonHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
              HighlightingLoader.Load(reader, HighlightingManager.Instance);
        }
      }
      HighlightingManager.Instance.RegisterHighlighting("Python Highlighting",
          new string[] { ".cool" },
          pythonHighlighting);
      avalonEdit.SyntaxHighlighting = pythonHighlighting;
    }

    public void ShowFile(string filename)
    {
      if (filename == CurrentFile) {
      }
      else
      {
        Dispatcher.Invoke((Action)(() =>
        {
          avalonEdit.Load(filename);
        }));
        CurrentFile = filename;
      }
      
    }

    public void TracebackEvent(object sender, IPYTracebackEventArgs e)
    {
      FunctionCode code = e.frame.f_code;

      string filename = code.co_filename;

      ShowFile(filename);

      try
      {
        switch (e.result)
        {
          case "call":
            TracebackCall(e);
            break;

          case "line":
            TracebackLine(e);
            break;

          case "return":
            TracebackReturn(e);
            break;

          default:
            break;
        }
      }
      catch (Exception ex)
      {
        Dispatcher.Invoke((Action)(() =>
        {
          ExceptionViewer ev = new ExceptionViewer("TracebackEvent", ex);
          ev.ShowDialog();
        }));
      }
    }

    private void TracebackCall(IPYTracebackEventArgs e)
    {
      if (e.frame != null)
      {
        int lineNo = (int)e.frame.f_lineno;
        HighlightLine(lineNo, Brushes.LightGreen, Brushes.Black);
      }
    }

    private void TracebackReturn(IPYTracebackEventArgs e)
    {
      int lineNo = (int)e.frame.f_code.co_firstlineno;
      HighlightLine(lineNo, Brushes.LightPink, Brushes.Black);
    }

    private void TracebackLine(IPYTracebackEventArgs e)
    {
      int lineNo = (int)e.frame.f_lineno;
      HighlightLine(lineNo, Brushes.Yellow, Brushes.Black);
    }
  }
}
