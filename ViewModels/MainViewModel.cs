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
using System.Text.RegularExpressions;
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
                LastInstruction = EDebugInstructionState.NULL;
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

            if (mBreakPointInfoOrNull.Count > 0)
            {
                LastInstruction = EDebugInstructionState.ADD_BREAK_POINT;
                string fileName = Path.GetFileName(mSourcePathOrNull);

                for (uint line = 1; line < mBreakPointInfoOrNull.Indices.Length; ++line)
                {
                    if (mBreakPointInfoOrNull.Indices[line] < uint.MaxValue)
                    {
                        SendInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_SOURCE_LEVEL, fileName, line));
                    }
                }
            }
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

                    {// line and code
                        Regex rx = new Regex(@"^>\s*(\d*):\s(.+)$");
                        Match match = rx.Match(data);

                        if (match.Success)
                        {
                            Debug.WriteLine("Line {0}: `{1}`", match.Groups[1].Value, match.Groups[2].Value);

                            uint line = 0;
                            uint.TryParse(match.Groups[1].Value, out line);
                            Debug.Assert(line > 0);
                            mLinePointer = line;
                            break;
                        }
                    }

                    {// stack trace
                        string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);
                        Regex rx = new Regex(@"^\d+\s[(Inline)|0-9a-f]{8}\s[--------|0-9a-f]{8}\s" + fileNameOnly + @"!(.*)\s\[(.*)\s@\s(\d+)\]");
                        //Regex rx = new Regex($"^\\d+\\s[(Inline)|0-9a-f]{8}\\s[--------|0-9a-f]{8}\\s{fileNameOnly}!(.*)\\s\\[(.*)\\s@\\s(\\d+)\\]");
                        Match match = rx.Match(data);

                        if (match.Success)
                        {
                            Debug.WriteLine("Function Name: {0}, Line: {1}", match.Groups[1].Value, match.Groups[3].Value);
                            break;
                        }
                    }

                    {
                        Regex rx = new Regex(@"^prv\s(local|param)\s+([0-9a-f]{8})\s+(.*)\s=\s(.*)$");
                        Match match = rx.Match(data);

                        if (match.Success)
                        {
                            lastIndex = match.Groups[3].Value.LastIndexOf(' ');
                            string type = match.Groups[3].Value.Substring(0, lastIndex);
                            string name = match.Groups[3].Value.Substring(lastIndex + 1);

                            Debug.WriteLine("Memory: {0}, Address: {1}, Type: {2}, Name: {3}, Value: {4}",
                                match.Groups[1].Value, match.Groups[2].Value, type, name, match.Groups[4].Value);
                            break;
                        }
                    }
                    break;

                case EDebugInstructionState.ADD_BREAK_POINT:
                    // intentional fallthrough
                case EDebugInstructionState.REMOVE_BREAK_POINT:
                    {
                        Regex rx = new Regex(@"^\s?(\d+)\se\s[0-9a-f]{8}\s\[(.+|:|\\)\s@\s(\d+)\]");
                        Match match = rx.Match(data);

                        if (match.Success)
                        {
                            uint bpIndex = uint.MaxValue;
                            Debug.Assert(uint.TryParse(match.Groups[1].Value, out bpIndex));
                            Debug.Assert(bpIndex < uint.MaxValue);

                            uint lineNumber = 0;
                            Debug.Assert(uint.TryParse(match.Groups[3].Value, out lineNumber));
                            Debug.Assert(lineNumber > 0);

                            ++mBreakPointInfoOrNull.Count;
                            BreakPointInfoOrNull.Indices[lineNumber] = bpIndex;
                        }
                    }
                    break;

                case EDebugInstructionState.NULL:
                    return;

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
