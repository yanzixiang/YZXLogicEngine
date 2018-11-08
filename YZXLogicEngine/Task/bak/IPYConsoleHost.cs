using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;

using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Providers;
using Microsoft.Scripting.Hosting.Shell;

using IronPython.Runtime.Exceptions;
using IronPython.Hosting;
using IronPython.Runtime;

namespace YZXLogicEngine.Task
{
    public delegate void ConsoleCreatedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Hosts the python console.
    /// </summary>
    public class IPYConsoleHost : ConsoleHost, IDisposable
    {
        Thread thread;
        IPYConsole pythonConsole;

        public event ConsoleCreatedEventHandler ConsoleCreated;

        public IPYConsoleHost()
        {
            
        }

        private TracebackDelegate OnTracebackReceived(TraceBackFrame frame, string result, object payload)
        {
            System.Console.WriteLine(frame.f_lineno);
            return OnTracebackReceived;
        }

        public IPYConsole Console
        {
            get { return pythonConsole; }
        }

        protected override Type Provider
        {
            get { return typeof(PythonContext); }
        }

        /// <summary>
        /// Runs the console host in its own thread.
        /// </summary>
        public void Run()
        {
            thread = new Thread(RunConsole);
            thread.IsBackground = true;
            thread.Start();
        }

        public void Dispose()
        {
            if (pythonConsole != null)
            {
                pythonConsole.Dispose();
            }

            if (thread != null)
            {
                thread.Join();
            }
        }

        protected override CommandLine CreateCommandLine()
        {
            return new PythonCommandLine();
        }

        protected override OptionsParser CreateOptionsParser()
        {
            return new PythonOptionsParser();
        }

        /// <remarks>
        /// After the engine is created the standard output is replaced with our custom Stream class so we
        /// can redirect the stdout to the text editor window.
        /// This can be done in this method since the Runtime object will have been created before this method
        /// is called.
        /// </remarks>
        protected override IConsole CreateConsole(ScriptEngine engine, CommandLine commandLine, ConsoleOptions options)
        {
            //SetOutput(new PythonOutputStream(textEditor));
            pythonConsole = new IPYConsole(commandLine);
            if (ConsoleCreated != null) ConsoleCreated(this, EventArgs.Empty);
            Runtime.SetTrace(OnTracebackReceived);
            return pythonConsole;
        }

        //protected virtual void SetOutput(PythonOutputStream stream)
        //{
        //    Runtime.IO.SetOutput(stream, Encoding.UTF8);
        //}

        /// <summary>
        /// Runs the console.
        /// </summary>
        void RunConsole()
        {
            this.Run(new string[] { "-X:FullFrames" });
        }

        protected override ScriptRuntimeSetup CreateRuntimeSetup()
        {
            ScriptRuntimeSetup srs = ScriptRuntimeSetup.ReadConfiguration();
            foreach (var langSetup in srs.LanguageSetups)
            {
                if (langSetup.FileExtensions.Contains(".py"))
                {
                    langSetup.Options["SearchPaths"] = new string[0];
                }
            }
            return srs;
        }

        protected override void ParseHostOptions(string[] args)
        {
            // Python doesn't want any of the DLR base options.
            foreach (string s in args)
            {
                Options.IgnoredArgs.Add(s);
            }
        }

        protected override void ExecuteInternal()
        {
            PythonContext pc = HostingHelpers.GetLanguageContext(Engine) as PythonContext;
            pc.SetModuleState(typeof(ScriptEngine), Engine);
            base.ExecuteInternal();
        }
    }
}
