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
        public ICommand ResumeCommand { get; }
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

        public EDebugInstructionState Instruction { get; set; }

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
            ResumeCommand = new ResumeCommand(this);
            StepOverCommand = new StepOverCommand(this);
            StepInCommand = new StepInCommand(this);
            BreakPointCommand = new BreakPointCommand(this);
        }

        public void ExecuteCdb(ProcessStartInfo processInfo)
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

        private void cmd()
        {
            mProcessCdbOrNull.Start();
            mProcessCdbOrNull.BeginOutputReadLine();
            mProcessCdbOrNull.BeginErrorReadLine();

            string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);

            SendInstruction(".expr /s c++");
            SendInstruction(".lines -e");
            SendInstruction("l+*");
            SendInstruction(".settings set Sources.SkipCrtCode=true"); // https://docs.microsoft.com/en-us/windows-hardware/drivers/debugger/-settings--set-debug-settings-
            SendInstruction($"bu {fileNameOnly}!main");
            SendInstruction("g");
        }

        private void onOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            int lastIndex = e.Data.LastIndexOf("0:000> "); // prevent duplicate string
            if (lastIndex != -1)
            {
                Log += e.Data.Substring(lastIndex);
            }
            else
            {
                Log += e.Data;
            }
            Log += Environment.NewLine;
        }

        private void onErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
            //Debug.Assert(false);
        }
    }
}
