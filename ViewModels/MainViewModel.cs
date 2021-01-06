using CppMemoryVisualizer.Commands;
using CppMemoryVisualizer.Constants;
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
        public event PropertyChangedEventHandler LinePointerChanged;

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

        private EDebugInstructionState mLastInstruction = EDebugInstructionState.STANDBY;

        private EDebugInstructionState mCurrentInstruction = EDebugInstructionState.STANDBY;
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
                OnPropertyChanged("CurrentInstruction");
            }
        }

        private EStandardCppVersion mStandardCppVersion = EStandardCppVersion.CPP17;
        public EStandardCppVersion StandardCppVersion
        {
            get
            {
                return mStandardCppVersion;
            }
            set
            {
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
                LinePointerChanged?.Invoke(this, new PropertyChangedEventArgs("LinePointer"));
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
            mProcessCdbOrNull.Start();

            mCallStackOrNull = new CallStack();

            CurrentInstruction = EDebugInstructionState.INIT;

            #region Initialize options and Set breakpoint in main function
            {
                RequestInstruction(CdbInstructionSet.CPP_EXPRESSION_EVALUATOR,
                    CdbInstructionSet.REQUEST_START_INIT, null);
                RequestInstruction(CdbInstructionSet.ENABLE_SOURCE_LINE_SUPPORT,
                    null, null);
                RequestInstruction(CdbInstructionSet.SET_SOURCE_OPTIONS,
                    null, null);
                RequestInstruction(CdbInstructionSet.SET_DEBUG_SETTINGS_SKIP_CRT_CODE,
                    null, null);
                RequestInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_MAIN, fileNameOnly),
                    null, CdbInstructionSet.REQUEST_END_INIT);
                ReadResultLine(CdbInstructionSet.REQUEST_START_INIT, CdbInstructionSet.REQUEST_END_INIT, (string line) =>
                {
                    Debug.WriteLine(line);
                });
            }
            #endregion

            GoCommand.Execute(null);
            CurrentInstruction = EDebugInstructionState.INIT;

            #region Remove breakpoint
            {
                RequestInstruction(string.Format(CdbInstructionSet.CLEAR_BREAK_POINT_MAIN, fileNameOnly),
                    CdbInstructionSet.REQUEST_START_INIT, CdbInstructionSet.REQUEST_END_INIT);
                ReadResultLine(CdbInstructionSet.REQUEST_START_INIT, CdbInstructionSet.REQUEST_END_INIT, (string line) =>
                {
                    Debug.WriteLine(line);
                });
            }
            #endregion

            CurrentInstruction = EDebugInstructionState.STANDBY;
        }

        public void ShutdownCdb()
        {
            if (mProcessCdbOrNull != null)
            {
                CurrentInstruction = EDebugInstructionState.STANDBY;
                RequestInstruction(CdbInstructionSet.QUIT,
                    null, null);
                ProcessCdbOrNull = null;
            }

            Log = string.Empty;
        }

        public void RequestInstruction(string instructionOrNull, string startOrNull, string endOrNull)
        {
            if (mProcessCdbOrNull != null)
            {
                if (startOrNull != null)
                {
                    mProcessCdbOrNull.StandardInput.WriteLine(string.Format(CdbInstructionSet.ECHO, startOrNull));
                }
                if (instructionOrNull != null)
                {
                    mProcessCdbOrNull.StandardInput.WriteLine(instructionOrNull);
                }
                if (endOrNull != null)
                {
                    mProcessCdbOrNull.StandardInput.WriteLine(string.Format(CdbInstructionSet.ECHO, endOrNull));
                }
            }
        }

        public void ReadResultLine(string start, string end, Action<string> lambdaOrNull)
        {
            Debug.Assert(start != null);
            Debug.Assert(end != null);

            string line;

            do
            {
                line = mProcessCdbOrNull.StandardOutput.ReadLine();
                {
                    int lastIndex = line.LastIndexOf(CdbInstructionSet.OUTPUT_HEADER);
                    if (lastIndex != -1)
                    {
                        line = line.Substring(lastIndex + CdbInstructionSet.OUTPUT_HEADER.Length);
                    }
                    if (line.Length == 0)
                    {
                        continue;
                    }
                }

                Log += line + Environment.NewLine;

            } while (!line.StartsWith(start));

            while (true)
            {
                line = mProcessCdbOrNull.StandardOutput.ReadLine();
                {
                    int lastIndex = line.LastIndexOf(CdbInstructionSet.OUTPUT_HEADER);
                    if (lastIndex != -1)
                    {
                        line = line.Substring(lastIndex + CdbInstructionSet.OUTPUT_HEADER.Length);
                    }
                    if (line.Length == 0)
                    {
                        continue;
                    }
                }

                Log += line + Environment.NewLine;

                if (line.StartsWith(end))
                {
                    break;
                }

                if (lambdaOrNull != null)
                {
                    lambdaOrNull.Invoke(line);
                }
            } 
        }

        public void Update()
        {
            #region Get StackTrace
            {
                mCallStackOrNull.Clear();

                RequestInstruction(CdbInstructionSet.DISPLAY_STACK_BACKTRACE,
                    CdbInstructionSet.REQUEST_START_GET_CALL_STACK, CdbInstructionSet.REQUEST_END_GET_CALL_STACK);
                ReadResultLine(CdbInstructionSet.REQUEST_START_GET_CALL_STACK, CdbInstructionSet.REQUEST_END_GET_CALL_STACK, (string line) => 
                {
                    string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);
                    Regex rx = new Regex(@"^\d+\s([0-9a-f]{8})\s([0-9a-f]{8})\s" + fileNameOnly + @"!(.*)\s\[(.*)\s@\s(\d+)\]");
                    Match match = rx.Match(line);

                    if (match.Success)
                    {
                        //string stackAddress = match.Groups[1].Value;
                        string functionAddress = match.Groups[2].Value;
                        string name = match.Groups[3].Value;
                        string path = match.Groups[4].Value;
                        //string line = match.Groups[5].Value;

                        if (path == mSourcePathOrNull)
                        {
                            uint functionAddr = 0;
                            Debug.Assert(uint.TryParse(functionAddress, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out functionAddr));
                            Debug.Assert(functionAddr > 0);

                            Debug.WriteLine("Function addr: {0}, name: {1}", functionAddr, name);
                            mCallStackOrNull.Push(functionAddr, name);
                        }
                    }
                });
            }
            #endregion

            #region Get Local Variable Info
            {
                RequestInstruction(CdbInstructionSet.DISPLAY_LOCAL_VARIABLE,
                    CdbInstructionSet.REQUEST_START_GET_LOCAL_VARS, CdbInstructionSet.REQUEST_END_GET_LOCAL_VARS);
                ReadResultLine(CdbInstructionSet.REQUEST_START_GET_LOCAL_VARS, CdbInstructionSet.REQUEST_END_GET_LOCAL_VARS, (string line) =>
                {
                    Regex rx = new Regex(@"^prv\s(local|param)\s+([0-9a-f]{8})\s+(.*)\s=\s(.*)$");
                    Match match = rx.Match(line);

                    if (match.Success)
                    {
                        int lastIndex = match.Groups[3].Value.LastIndexOf(' ');
                        string name = match.Groups[3].Value.Substring(lastIndex + 1);

                        CppMemoryVisualizer.Models.StackFrame stackFrame = mCallStackOrNull.GetStackFrame(mCallStackOrNull.Top());
                        stackFrame.TryAdd(name);
                        LocalVariable local = stackFrame.GetLocalVariable(name);

                        // type (fix)
                        local.StackMemory.Type = match.Groups[3].Value.Substring(0, lastIndex);

                        // name (fix)
                        local.Name = name;

                        // address
                        uint address = 0;
                        Debug.Assert(uint.TryParse(match.Groups[2].Value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out address));
                        local.StackMemory.Address = address;
                    }
                });
            }
            #endregion

            /*
            // Check if initialized
            if (!mCallStackOrNull.IsEmpty())
            {
                CppMemoryVisualizer.Models.StackFrame stackFrame = mCallStackOrNull.GetStackFrame(mCallStackOrNull.Top());
                if (stackFrame.IsInitialized)
                {
                    goto UpdateMemory;
                }
            }
            */

            #region Get Local Variable SizeOf
            {
                CppMemoryVisualizer.Models.StackFrame stackFrame = mCallStackOrNull.GetStackFrame(mCallStackOrNull.Top());

                foreach (var name in stackFrame.LocalVariableNames)
                {
                    RequestInstruction(string.Format(CdbInstructionSet.EVALUATE_SIZEOF, name),
                        CdbInstructionSet.REQUEST_START_SIZEOF + ' ' + name, CdbInstructionSet.REQUEST_END_SIZEOF);
                }

                foreach (var name in stackFrame.LocalVariableNames)
                {
                    ReadResultLine(CdbInstructionSet.REQUEST_START_SIZEOF, CdbInstructionSet.REQUEST_END_SIZEOF, (string line) =>
                    {
                        int lastIndex = line.LastIndexOf(' ');
                        Debug.Assert(lastIndex >= 0);

                        string value = line.Substring(lastIndex + 1);
                        uint size = uint.MaxValue;

                        if (value.StartsWith("0x")) // hexadecimal
                        {
                            Debug.Assert(uint.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out size));
                        }
                        else // decimal
                        {
                            Debug.Assert(uint.TryParse(value, out size));
                        }
                        Debug.Assert(size < uint.MaxValue);

                        LocalVariable local = stackFrame.GetLocalVariable(name);

                        local.StackMemory.Size = size;

                        uint wordSize = size / 4 + (uint)(size % 4 > 0 ? 1 : 0);

                        if (local.StackMemory.ByteValues == null)
                        {
                            local.StackMemory.ByteValues = new byte[wordSize * 4];
                        }
                    });
                }
                stackFrame.IsInitialized = true;
            }

            #endregion

UpdateMemory:

            #region Get Memory Word Pattern
            {
                CppMemoryVisualizer.Models.StackFrame stackFrame = mCallStackOrNull.GetStackFrame(mCallStackOrNull.Top());

                foreach (var name in stackFrame.LocalVariableNames)
                {
                    LocalVariable local = stackFrame.GetLocalVariable(name);
                    RequestInstruction(string.Format(CdbInstructionSet.DISPLAY_MEMORY, local.StackMemory.ByteValues.Length / 4, "0x" + local.StackMemory.Address.ToString("X")),
                        CdbInstructionSet.REQUEST_START_DISPLAY_MEMORY + ' ' + name, CdbInstructionSet.REQUEST_END_DISPLAY_MEMORY);
                }

                foreach (var name in stackFrame.LocalVariableNames)
                {
                    LocalVariable local = stackFrame.GetLocalVariable(name);
                    ReadResultLine(CdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, CdbInstructionSet.REQUEST_END_DISPLAY_MEMORY, (string line) =>
                    {
                        local.StackMemory.SetValue(line);
                    });
                }
            }
            #endregion
        }

        public void ActionLinePointer(string line)
        {
            Regex rx = new Regex(@"^>\s*(\d*):\s(.+)$");
            Match match = rx.Match(line);

            if (match.Success)
            {
                Debug.WriteLine("Line {0}: `{1}`", match.Groups[1].Value, match.Groups[2].Value);

                uint lineNumber = 0;
                uint.TryParse(match.Groups[1].Value, out lineNumber);
                Debug.Assert(lineNumber > 0);
                LinePointer = lineNumber;
            }
            else if (line.StartsWith("ntdll!NtTerminateProcess"))
            {
                Debug.WriteLine("Program is terminated.");
                LinePointer = 0;
            }
        }
    }
}
