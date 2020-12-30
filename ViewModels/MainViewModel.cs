using CppMemoryVisualizer.Commands;
using CppMemoryVisualizer.Enums;
using CppMemoryVisualizer.Models;
using CppMemoryVisualizer.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace CppMemoryVisualizer.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event DataReceivedEventHandler MarginOnOutputDataReceived;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand LoadSourceFileCommand { get; }
        public ICommand DebugCommand { get; }
        public ICommand GoCommand { get; }
        public ICommand StepOverCommand { get; }
        public ICommand StepInCommand { get; }
        public ICommand AddOrRemoveBreakPointCommand { get; }

        private Process mProcessCdbOrNull;
        public Process ProcessCdbOrNull
        {
            get
            {
                return mProcessCdbOrNull;
            }
            set
            {
                mProcessCdbOrNull = value;
                OnPropertyChanged("ProcessCdbOrNull");
            }
        }

        private Thread mThreadCdbOrNull;
        public Thread ThreadCdbOrNull
        {
            get
            {
                return mThreadCdbOrNull;
            }
            set
            {
                mThreadCdbOrNull = value;
                OnPropertyChanged("ThreadCdbOrNull");
            }
        }

        public EDebugInstructionState LastInstruction { get; set; }

        private EStandardCppVersion mStandardCppVersion = EStandardCppVersion.CPP17;
        public EStandardCppVersion StandardCppVersion
        {
            get { return mStandardCppVersion; }
            set { mStandardCppVersion = value; OnPropertyChanged("StandardCppVersion"); }
        }

        private string mLog;
        public string Log
        {
            get
            {
                return mLog;
            }
            set
            {
                mLog = value;
                OnPropertyChanged("Log");
            }
        }

        private string mSourcePathOrNull;
        public string SourcePathOrNull
        {
            get { return mSourcePathOrNull; }
            set
            {
                mSourcePathOrNull = value;
                OnPropertyChanged("SourcePathOrNull");
            }
        }

        private string mSourceCode = string.Empty;
        public string SourceCode
        {
            get { return mSourceCode; }
            set
            {
                mSourceCode = value;
                OnPropertyChanged("SourceCode");
            }
        }

        private BreakPointInfo mBreakPointInfoOrNull;
        public BreakPointInfo BreakPointInfoOrNull
        {
            get { return mBreakPointInfoOrNull; }
            set { mBreakPointInfoOrNull = value; OnPropertyChanged("BreakPointInfoOrNull"); }
        }

        private uint mLinePointer;
        public uint LinePointer
        {
            get { return mLinePointer; }
            set { mLinePointer = value; OnPropertyChanged("LinePointer"); }
        }

        public MainViewModel()
        {
            LoadSourceFileCommand = new LoadSourceFileCommand(this);
            DebugCommand = new DebugCommand(this);
            GoCommand = new GoCommand(this);
            StepOverCommand = new StepOverCommand(this);
            StepInCommand = new StepInCommand(this);
            AddOrRemoveBreakPointCommand = new AddOrRemoveBreakPointCommand(this);
        }

        public void ExecuteCdb()
        {
            mProcessCdbOrNull.OutputDataReceived += onOutputDataReceived;
            mProcessCdbOrNull.OutputDataReceived += MarginOnOutputDataReceived;
            mProcessCdbOrNull.ErrorDataReceived += onErrorDataReceived;

            ThreadCdbOrNull = new Thread(new ThreadStart(cmd));
            ThreadCdbOrNull.Start();
        }

        public void SendInstruction(string instruction)
        {
            Debug.Assert(instruction != null);
            
            if (ThreadCdbOrNull != null)
            {
                mProcessCdbOrNull.StandardInput.WriteLine(instruction);
            }
        }

        public void ShutdownCdb()
        {
            if (ProcessCdbOrNull != null)
            {
                SendInstruction(CdbInstructionSet.QUIT);
                ProcessCdbOrNull = null;
            }

            if (ThreadCdbOrNull != null)
            {
                ThreadCdbOrNull.Join();
                ThreadCdbOrNull = null;
            }

            Log = string.Empty;
        }

        private void cmd()
        {
            mProcessCdbOrNull.Start();
            mProcessCdbOrNull.BeginOutputReadLine();
            mProcessCdbOrNull.BeginErrorReadLine();

            string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);

            SendInstruction(CdbInstructionSet.CPP_EXPRESSION_EVALUATOR);
            SendInstruction(CdbInstructionSet.ENABLE_SOURCE_LINE_SUPPORT);
            SendInstruction(CdbInstructionSet.SET_SOURCE_OPTIONS);
            SendInstruction(CdbInstructionSet.SET_DEBUG_SETTINGS_SKIP_CRT_CODE);
            SendInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_MAIN, fileNameOnly));

            LastInstruction = EDebugInstructionState.GO;
            SendInstruction(CdbInstructionSet.GO);
            SendInstruction(string.Format(CdbInstructionSet.CLEAR_BREAK_POINT_MAIN, fileNameOnly));
            SendInstruction(CdbInstructionSet.DISPLAY_STACK_BACKTRACE);
            SendInstruction(CdbInstructionSet.DISPLAY_LOCAL_VARIABLE);
        }

        private void onOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            string data = e.Data;

            int lastIndex = data.LastIndexOf("0:000> ");
            if (lastIndex != -1)
            {
                data = data.Substring(lastIndex + 7);
            }

            if (data.Length == 0 || data == "quit:")
            {
                return;
            }

            switch (LastInstruction)
            {
                case EDebugInstructionState.STEP_IN:
                    // intentional fallthrough
                case EDebugInstructionState.STEP_OVER:
                    // intentional fallthrough
                case EDebugInstructionState.GO:
                    if (data[0] != '>')
                    {
                        break;
                    }

                    uint line = 0;
                    UInt32.TryParse(data.Substring(1, 5).Trim(), out line);
                    Debug.Assert(line > 0);
                    
                    string code = data.Substring(8);

                    Debug.WriteLine("Line {0}: `{1}`", line, code);
                    mLinePointer = line;

                    break;

                case EDebugInstructionState.ADD_BREAK_POINT:
                    // intentional fallthrough
                case EDebugInstructionState.REMOVE_BREAK_POINT:
                    mBreakPointInfoOrNull.ProcessLine(data);
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }

            Log += data;
            Log += Environment.NewLine;
        }

        private void onErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
        }
    }
}
