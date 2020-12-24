using CppMemoryVisualizer.Commands;
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
        public ICommand BreakPointCommand { get; }

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

        public MainViewModel()
        {
            LoadSourceFileCommand = new LoadSourceFileCommand(this);
            DebugCommand = new DebugCommand(this);
            GoCommand = new GoCommand(this);
            StepOverCommand = new StepOverCommand(this);
            StepInCommand = new StepInCommand(this);
            BreakPointCommand = new BreakPointCommand(this);
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

            switch (LastInstruction)
            {
                case EDebugInstructionState.STEP_IN:
                case EDebugInstructionState.STEP_OVER:
                case EDebugInstructionState.GO:
                    if (data.Length == 0 || data[0] != '>')
                    {
                        break;
                    }

                    uint line = 0;
                    UInt32.TryParse(data.Substring(1, 5).Trim(), out line);
                    Debug.Assert(line > 0);
                    
                    string code = data.Substring(8);

                    Debug.WriteLine("Line {0}: `{1}`", line, code);

                    break;

                case EDebugInstructionState.BREAK_POINT:
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
