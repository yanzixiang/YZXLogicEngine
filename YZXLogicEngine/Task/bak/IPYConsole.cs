using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Documents;
using System.Threading;
using System.Windows.Input;

using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Hosting.Shell;
using Microsoft.Scripting.Hosting.Providers;
using Style = Microsoft.Scripting.Hosting.Shell.Style;

using IronPython.Hosting;
using IronPython.Runtime;



namespace YZXLogicEngine.Task
{
    public delegate void ConsoleInitializedEventHandler(object sender, EventArgs e);
    
    /// <summary>
    /// Custom IronPython console. The command dispacher runs on a separate UI thread from the REPL
    /// and also from the WPF control.
    /// </summary>
    public class IPYConsole : IConsole, IDisposable
    {
        int lineReceivedEventIndex = 0; // The index into the waitHandles array where the lineReceivedEvent is stored.
        ManualResetEvent lineReceivedEvent = new ManualResetEvent(false);
        ManualResetEvent disposedEvent = new ManualResetEvent(false);
        AutoResetEvent statementsExecutionRequestedEvent = new AutoResetEvent(false);
        WaitHandle[] waitHandles;
        int promptLength = 4;
        List<string> previousLines = new List<string>();
        CommandLine commandLine;

        volatile bool executing = false;

        // This is the thread upon which all commands execute unless the dipatcher is overridden.
        Thread dispatcherThread;
        Window dispatcherWindow;
        Dispatcher dispatcher;

        string scriptText = String.Empty;
        bool consoleInitialized = false;
        string prompt;
      
        public event ConsoleInitializedEventHandler ConsoleInitialized;

        public ScriptScope ScriptScope
        {
            get { return commandLine.ScriptScope; }
        }

        public IPYConsole(CommandLine commandLine)
        {   
            waitHandles = new WaitHandle[] { lineReceivedEvent, disposedEvent };

            this.commandLine = commandLine;
            dispatcherThread = new Thread(new ThreadStart(DispatcherThreadStartingPoint));
            dispatcherThread.SetApartmentState(ApartmentState.STA);
            dispatcherThread.IsBackground = true;
            dispatcherThread.Start();

            // Only required when running outside REP loop.
            prompt = ">>> ";

            CodeContext codeContext = DefaultContext.Default;
            // Set dispatcher to run on a UI thread independent of both the Control UI thread and thread running the REPL.
            ClrModule.SetCommandDispatcher(codeContext, DispatchCommand);
        }

        protected void DispatchCommand(Delegate command)
        {
            if (command != null)
            {
                // Slightly involved form to enable keyboard interrupt to work.
                executing = true;
                var operation = dispatcher.BeginInvoke(DispatcherPriority.Normal, command);
                while (executing)
                {
                    if (operation.Status != DispatcherOperationStatus.Completed) 
                        operation.Wait(TimeSpan.FromSeconds(1));
                    if (operation.Status == DispatcherOperationStatus.Completed)
                        executing = false;
                }
            }
        }

        private void DispatcherThreadStartingPoint()
        {
            dispatcherWindow = new Window();
            dispatcher = dispatcherWindow.Dispatcher;
            while (true)
            {
                try
                {
                    System.Windows.Threading.Dispatcher.Run();
                }
                catch (ThreadAbortException tae)
                {
                    if (tae.ExceptionState is Microsoft.Scripting.KeyboardInterruptException)
                    {
                        Thread.ResetAbort();
                        executing = false;
                    }
                }
            }
        }

        public void Dispose()
        {
            disposedEvent.Set();
        }

        public TextWriter Output
        {
            get { return null; }
            set { }
        }

        public TextWriter ErrorOutput
        {
            get { return null; }
            set { }
        }

        /// <summary>
        /// Run externally provided statements in the Console Engine. 
        /// </summary>
        /// <param name="statements"></param>
        public void RunStatements(string statements)
        {
            lock (this.scriptText)
            {
                this.scriptText = statements;
            }
            dispatcher.BeginInvoke(new Action(delegate() { ExecuteStatements(); }));
        }

        /// <summary>
        /// Run on the statement execution thread. 
        /// </summary>
        void ExecuteStatements()
        {
            lock (scriptText)
            {
                ScriptSource scriptSource = commandLine.ScriptScope.Engine.CreateScriptSourceFromString(scriptText, SourceCodeKind.Statements);
                string error = "";
                try
                {
                    executing = true;
                    scriptSource.Execute(commandLine.ScriptScope);
                }
                catch (ThreadAbortException tae)
                {
                    if (tae.ExceptionState is Microsoft.Scripting.KeyboardInterruptException) Thread.ResetAbort();
                    error = "KeyboardInterrupt" + System.Environment.NewLine;
                }
                catch (Microsoft.Scripting.SyntaxErrorException exception)
                {
                    ExceptionOperations eo;
                    eo = commandLine.ScriptScope.Engine.GetService<ExceptionOperations>();
                    error = eo.FormatException(exception);
                }
                catch (Exception exception)
                {
                    ExceptionOperations eo;
                    eo = commandLine.ScriptScope.Engine.GetService<ExceptionOperations>();
                    error = eo.FormatException(exception) + System.Environment.NewLine;
                }
                executing = false;
            }
        }

        /// <summary>
        /// Returns the next line typed in by the console user. If no line is available this method
        /// will block.
        /// </summary>
        public string ReadLine(int autoIndentSize)
        {
            string indent = String.Empty;
            if (autoIndentSize > 0)
            {
                indent = String.Empty.PadLeft(autoIndentSize);
                Write(indent, Style.Prompt);
            }

            //string line = ReadLineFromTextEditor();
            string line = "";
            if (line != null)
            {
                return indent + line;
            }
            return null;
        }

        /// <summary>
        /// Writes text to the console.
        /// </summary>
        public void Write(string text, Style style)
        {
            //textEditor.Write(text);
            if (style == Style.Prompt)
            {
                promptLength = text.Length;
                if (!consoleInitialized)
                {
                    consoleInitialized = true;
                    if (ConsoleInitialized != null) ConsoleInitialized(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Writes text followed by a newline to the console.
        /// </summary>
        public void WriteLine(string text, Style style)
        {
            Write(text + Environment.NewLine, style);
        }

        /// <summary>
        /// Writes an empty line to the console.
        /// </summary>
        public void WriteLine()
        {
            Write(Environment.NewLine, Style.Out);
        }
    }
}
