using CppMemoryVisualizer.Commands;
using CppMemoryVisualizer.Enums;
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

namespace CppMemoryVisualizer.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

        private int[] mBreakPointLines;
        public int[] BreakPointLines
        {
            get { return mBreakPointLines; }
            set { mBreakPointLines = value; }
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

        public void ExecuteCdb(ProcessStartInfo processInfo)
        {
            ProcessCdbOrNull = new Process();
            ProcessCdbOrNull.StartInfo = processInfo;
            ProcessCdbOrNull.OutputDataReceived += onOutputDataReceived;
            ProcessCdbOrNull.ErrorDataReceived += onErrorDataReceived;

            ThreadCdbOrNull = new Thread(new ThreadStart(cmd));
            ThreadCdbOrNull.Start();
        }

        public bool SendInstruction(string instruction)
        {
            if (mProcessCdbOrNull != null)
            {
                mProcessCdbOrNull.StandardInput.WriteLine(instruction);
                return true;
            }

            return false;
        }

        public void ShutdownCdb()
        {
            if (ProcessCdbOrNull != null)
            {
                ProcessCdbOrNull.OutputDataReceived -= onOutputDataReceived;
                SendInstruction(CdbInstructionSet.QUIT);
                ProcessCdbOrNull.WaitForExit();
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

                    break;

                case EDebugInstructionState.ADD_BREAK_POINT:
                    // intentional fallthrough
                case EDebugInstructionState.REMOVE_BREAK_POINT:
                    string[] bpInfos = data.Split(' ');

                    int bpIndex = -1;
                    Debug.Assert(int.TryParse(bpInfos[1], out bpIndex));

                    uint lineNumber = 0;
                    Debug.Assert(uint.TryParse(bpInfos[6].Remove(bpInfos[6].Length - 1), out lineNumber));

                    mBreakPointLines[lineNumber] = bpIndex;
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

        public static void BreakPointMargin_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BreakPointMargin margin = sender as BreakPointMargin;
            BindableAvalonEditor editor = margin.Editor;
            MainViewModel viewModel = (MainViewModel)margin.DataContext;

            var positionOrNull = editor.GetPositionFromPoint(e.GetPosition(margin));
            if (positionOrNull == null)
            {
                return;
            }

            uint line = (uint)positionOrNull.Value.Location.Line;

            if (viewModel.AddOrRemoveBreakPointCommand.CanExecute(line))
            {
                viewModel.AddOrRemoveBreakPointCommand.Execute(line);
            }

            margin.InvalidateVisual();
        }
    }
}
