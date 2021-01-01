using CppMemoryVisualizer.Commands;
using CppMemoryVisualizer.Enums;
using CppMemoryVisualizer.Models;
using CppMemoryVisualizer.Views;
using System;
using System.Globalization;
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

        private EDebugInstructionState mLastInstruction = EDebugInstructionState.NULL;
        private EDebugInstructionState mCurrentInstruction = EDebugInstructionState.NULL;
        public EDebugInstructionState CurrentInstruction
        {
            get
            {
                return mCurrentInstruction;
            }
            set
            {
                mLastInstruction = mCurrentInstruction;
                mCurrentInstruction = value;
            }
        }

        private EStandardCppVersion mStandardCppVersion = EStandardCppVersion.CPP17;
        public EStandardCppVersion StandardCppVersion
        {
            get
            {
                return mStandardCppVersion;
            }
            set {
                mStandardCppVersion = value;
                OnPropertyChanged("StandardCppVersion");
            }
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
            get
            {
                return mSourcePathOrNull;
            }
            set
            {
                mSourcePathOrNull = value;
                OnPropertyChanged("SourcePathOrNull");
            }
        }

        private string mSourceCode = string.Empty;
        public string SourceCode
        {
            get
            {
                return mSourceCode;
            }
            set
            {
                mSourceCode = value;
                OnPropertyChanged("SourceCode");
            }
        }

        private BreakPointInfo mBreakPointInfoOrNull;
        public BreakPointInfo BreakPointInfoOrNull
        {
            get
            {
                return mBreakPointInfoOrNull;
            }
            set
            {
                mBreakPointInfoOrNull = value;
                OnPropertyChanged("BreakPointInfoOrNull");
            }
        }

        private uint mLinePointer;
        public uint LinePointer
        {
            get
            {
                return mLinePointer;
            }
            set
            {
                mLinePointer = value;
                OnPropertyChanged("LinePointer");
            }
        }

        private CallStack mCallStackOrNull;
        public CallStack CallStackOrNull
        {
            get
            {
                return mCallStackOrNull;
            }
        }

        private string mTargetLocalVariableName = string.Empty;

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
            string dirPath = Path.GetDirectoryName(mSourcePathOrNull);
            string fileName = Path.GetFileName(mSourcePathOrNull);
            string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);

            App app = Application.Current as App;

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = app.CdbPath;
            processInfo.WorkingDirectory = dirPath;
            processInfo.Arguments = $"-o {fileNameOnly}.exe -y {fileNameOnly}.pdb -srcpath {fileName}";
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = true;

            ProcessCdbOrNull = new Process();
            mProcessCdbOrNull.StartInfo = processInfo;

            mProcessCdbOrNull.OutputDataReceived += onOutputDataReceived;
            mProcessCdbOrNull.OutputDataReceived += MarginOnOutputDataReceived;
            mProcessCdbOrNull.ErrorDataReceived += onErrorDataReceived;

            mCallStackOrNull = new CallStack();

            ThreadCdbOrNull = new Thread(new ThreadStart(() =>
            {
                mProcessCdbOrNull.Start();
                mProcessCdbOrNull.BeginOutputReadLine();
                mProcessCdbOrNull.BeginErrorReadLine();

                SendInstruction(CdbInstructionSet.CPP_EXPRESSION_EVALUATOR);
                SendInstruction(CdbInstructionSet.ENABLE_SOURCE_LINE_SUPPORT);
                SendInstruction(CdbInstructionSet.SET_SOURCE_OPTIONS);
                SendInstruction(CdbInstructionSet.SET_DEBUG_SETTINGS_SKIP_CRT_CODE);
                SendInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_MAIN, fileNameOnly));

                CurrentInstruction = EDebugInstructionState.GO;
                SendInstruction(CdbInstructionSet.GO);
                SendInstruction(string.Format(CdbInstructionSet.CLEAR_BREAK_POINT_MAIN, fileNameOnly));
                SendInstruction(CdbInstructionSet.DISPLAY_STACK_BACKTRACE);
                SendInstruction(CdbInstructionSet.DISPLAY_LOCAL_VARIABLE);

                if (mBreakPointInfoOrNull.Count > 0)
                {
                    CurrentInstruction = EDebugInstructionState.ADD_BREAK_POINT;

                    for (uint line = 1; line < mBreakPointInfoOrNull.Indices.Length; ++line)
                    {
                        if (mBreakPointInfoOrNull.Indices[line] < uint.MaxValue)
                        {
                            SendInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_SOURCE_LEVEL, fileName, line));
                        }
                    }
                }
            }));

            ThreadCdbOrNull.Start();
        }

        public void ShutdownCdb()
        {
            if (mProcessCdbOrNull != null)
            {
                CurrentInstruction = EDebugInstructionState.NULL;
                SendInstruction(CdbInstructionSet.QUIT);
                ProcessCdbOrNull = null;
            }

            if (mThreadCdbOrNull != null)
            {
                mThreadCdbOrNull.Join();
                ThreadCdbOrNull = null;
            }

            Log = string.Empty;
        }

        public void SendInstruction(string instruction)
        {
            Debug.Assert(instruction != null);

            if (mThreadCdbOrNull != null)
            {
                mProcessCdbOrNull.StandardInput.WriteLine(instruction);
            }
        }

        private void onOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            string data = e.Data;

            {
                int lastIndex = data.LastIndexOf("0:000> ");
                if (lastIndex != -1)
                {
                    data = data.Substring(lastIndex + 7);
                }

                if (data.Length == 0)
                {
                    return;
                }
            }

            switch (CurrentInstruction)
            {
                case EDebugInstructionState.STEP_IN:
                    // intentional fallthrough
                case EDebugInstructionState.STEP_OVER:
                    // intentional fallthrough
                case EDebugInstructionState.GO:

                    #region Get sizeof
                    if (data.StartsWith(CdbInstructionSet.ECHO_GET_SIZEOF))
                    {
                        mTargetLocalVariableName = data.Substring(CdbInstructionSet.ECHO_GET_SIZEOF.Length);
                        CurrentInstruction = EDebugInstructionState.SIZEOF;
                        break;
                    }
                    #endregion

                    #region Get Line number and Code Line
                    {
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
                    #endregion

                    #region Get StackTrace
                    {
                        string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);
                        Regex rx = new Regex(@"^\d+\s([0-9a-f]{8})\s([0-9a-f]{8})\s" + fileNameOnly + @"!(.*)\s\[(.*)\s@\s(\d+)\]");
                        Match match = rx.Match(data);

                        if (match.Success)
                        {
                            //string stackAddress = match.Groups[1].Value;
                            string name = match.Groups[3].Value;
                            string path = match.Groups[4].Value;
                            //string line = match.Groups[5].Value;

                            uint functionAddr = 0;
                            uint.TryParse(match.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out functionAddr);
                            Debug.Assert(functionAddr > 0);

                            if (path == mSourcePathOrNull)
                            {
                                Debug.WriteLine("Function addr: {0}, name: {1}", functionAddr, name);
                                mCallStackOrNull.Push(functionAddr, name);
                            }

                            break;
                        }
                    }
                    #endregion

                    #region Get Local Variable Info
                    {
                        Regex rx = new Regex(@"^prv\s(local|param)\s+([0-9a-f]{8})\s+(.*)\s=\s(.*)$");
                        Match match = rx.Match(data);

                        if (match.Success)
                        {
                            int lastIndex = match.Groups[3].Value.LastIndexOf(' ');
                            string name = match.Groups[3].Value.Substring(lastIndex + 1);

                            CppMemoryVisualizer.Models.StackFrame stackFrame = mCallStackOrNull.GetStackFrame(mCallStackOrNull.Top());
                            stackFrame.TryAdd(name);

                            LocalVariable local = stackFrame.GetLocalVariable(name);
                            local.Address = match.Groups[2].Value;
                            local.Type = match.Groups[3].Value.Substring(0, lastIndex);
                            local.Name = name;
                            string oldValue = local.Value;
                            local.Value = match.Groups[4].Value;

                            Debug.WriteLine("Memory: {0}, Address: {1}, Type: {2}, Name: {3}, Value: {4}",
                                match.Groups[1].Value, local.Address, local.Type, local.Name, local.Value);

                            //SendInstruction($"dx {name}");

                            if (local.Size == uint.MaxValue)
                            {
                                SendInstruction(string.Format(CdbInstructionSet.ECHO, CdbInstructionSet.ECHO_GET_SIZEOF + name) + $";?? sizeof({name})");
                            }

                            if (oldValue != local.Value && local.Value.StartsWith("0x"))
                            {
                                SendInstruction($".echo \"!heap started ...\"");
                                SendInstruction($"!heap -x {local.Value}");
                            }

                            break;
                        }
                    }
                    #endregion
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

                case EDebugInstructionState.SIZEOF:
                    {
                        int lastIndex = data.LastIndexOf(' ');
                        string value = data.Substring(lastIndex + 1);
                        uint size = uint.MaxValue;
                        if (value.StartsWith("0x"))
                        {
                            uint.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out size);
                        }
                        else
                        {
                            uint.TryParse(value, out size);
                        }
                        Debug.Assert(size < uint.MaxValue);

                        CppMemoryVisualizer.Models.StackFrame stackFrame = mCallStackOrNull.GetStackFrame(mCallStackOrNull.Top());
                        LocalVariable local = stackFrame.GetLocalVariable(mTargetLocalVariableName);
                        local.Size = size;

                        CurrentInstruction = mLastInstruction;
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
