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
    sealed class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler LinePointerChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void OnLinePointerChanged()
        {
            LinePointerChanged?.Invoke(this, new PropertyChangedEventArgs("LinePointer"));
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

        private EDebugInstructionState mLastInstruction = EDebugInstructionState.DEAD;

        private EDebugInstructionState mCurrentInstruction = EDebugInstructionState.DEAD;
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
                OnLinePointerChanged();
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

        private TypeSizeManager mTypeSizeManagerOrNull;

        public readonly object LockObject = new object();

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
            Log = string.Empty;

            mProcessCdbOrNull.Start();

            CurrentInstruction = EDebugInstructionState.INITIALIZING;
            LinePointer = 0;
            mCallStackOrNull = new CallStack();
            mTypeSizeManagerOrNull = new TypeSizeManager();

            #region Initialize options
            {
                RequestInstruction(CdbInstructionSet.CPP_EXPRESSION_EVALUATOR,
                    CdbInstructionSet.REQUEST_START_INIT, null);
                RequestInstruction(CdbInstructionSet.ENABLE_SOURCE_LINE_SUPPORT,
                    null, null);
                RequestInstruction(CdbInstructionSet.SET_SOURCE_OPTIONS,
                    null, null);
                RequestInstruction(CdbInstructionSet.SET_DEBUG_SETTINGS_SKIP_CRT_CODE,
                    null, null);
            }
            #endregion

            #region
            {
                // Set breakpoint in main
                RequestInstruction(string.Format(CdbInstructionSet.SET_BREAK_POINT_MAIN, fileNameOnly),
                    null, CdbInstructionSet.REQUEST_END_INIT);
                ReadResultLine(CdbInstructionSet.REQUEST_START_INIT, CdbInstructionSet.REQUEST_END_INIT, (string line) =>
                {
                    Debug.WriteLine(line);
                });

                // Go
                RequestInstruction(CdbInstructionSet.GO,
                    CdbInstructionSet.REQUEST_START_GO_COMMAND, CdbInstructionSet.REQUEST_END_GO_COMMAND);
                ReadResultLine(CdbInstructionSet.REQUEST_START_GO_COMMAND, CdbInstructionSet.REQUEST_END_GO_COMMAND,
                    ActionLinePointer);

                // Remove breakpoint
                RequestInstruction(string.Format(CdbInstructionSet.CLEAR_BREAK_POINT_MAIN, fileNameOnly),
                    CdbInstructionSet.REQUEST_START_INIT, CdbInstructionSet.REQUEST_END_INIT);
                ReadResultLine(CdbInstructionSet.REQUEST_START_INIT, CdbInstructionSet.REQUEST_END_INIT, (string line) =>
                {
                    Debug.WriteLine(line);
                });
            }
            #endregion

            Update();

            CurrentInstruction = EDebugInstructionState.STANDBY;
        }

        public void ShutdownCdb()
        {
            if (mProcessCdbOrNull != null)
            {
                CurrentInstruction = EDebugInstructionState.DEAD;
                RequestInstruction(CdbInstructionSet.QUIT,
                    null, null);
                ProcessCdbOrNull = null;
            }
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
#if DEBUG
                Log += line + Environment.NewLine;
#endif

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
#if DEBUG
                Log += line + Environment.NewLine;
#endif
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

            if (mCallStackOrNull.IsEmpty())
            {
                LinePointer = 0;
                ShutdownCdb();
                return;
            }

            CppMemoryVisualizer.Models.StackFrame stackFrame = mCallStackOrNull.GetStackFrame(mCallStackOrNull.Top());

            #region Get Local Variable Info
            {
                RequestInstruction(CdbInstructionSet.DISPLAY_LOCAL_VARIABLE,
                    CdbInstructionSet.REQUEST_START_GET_LOCAL_VARS, CdbInstructionSet.REQUEST_END_GET_LOCAL_VARS);
                ReadResultLine(CdbInstructionSet.REQUEST_START_GET_LOCAL_VARS, CdbInstructionSet.REQUEST_END_GET_LOCAL_VARS, (string line) =>
                {
                    if (!stackFrame.IsInitialized)
                    {
                        Regex rx = new Regex(@"^prv\s(local|param)\s+([0-9a-f]{8})\s+(\w+|\w+\s[a-zA-Z0-9_<>,: ]+|<function>)\s(\**)([\(\*+\)]*)([\[\d+\]]*)\s*(\w+)\s=\s");
                        Match match = rx.Match(line);

                        if (match.Success)
                        {
                            string localOrParam = match.Groups[1].Value;
                            string stackAddr = match.Groups[2].Value;
                            string typeName = match.Groups[3].Value;
                            string pointerChars = match.Groups[4].Value;
                            string arrayOrFunctionPointerChars = match.Groups[5].Value;
                            string dimensions = match.Groups[6].Value;
                            string variableName = match.Groups[7].Value;

                            stackFrame.TryAdd(variableName);
                            LocalVariable local = stackFrame.GetLocalVariable(variableName);

                            // local or parameter
                            if (localOrParam == "param")
                            {
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.PARAMETER;
                            }

                            // address
                            uint address = 0;
                            Debug.Assert(uint.TryParse(stackAddr, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out address));
                            local.StackMemory.Address = address;

                            // typename (fixed)
                            if (typeName.StartsWith("struct "))
                            {
                                typeName = typeName.Substring(7);
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.STRUCT;
                            }
                            else if (typeName.StartsWith("class "))
                            {
                                typeName = typeName.Substring(6);
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.CLASS;
                            }
                            else if (typeName.StartsWith("enum "))
                            {
                                typeName = typeName.Substring(5);
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.ENUM;
                            }
                            else if (typeName.StartsWith("union "))
                            {
                                typeName = typeName.Substring(6);
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.UNION;
                            }
                            else if (typeName == "<function>")
                            {
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.FUNCTION;
                            }

                            // STL
                            if (typeName.Contains("std::"))
                            {
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.STL;
                            }

                            // TypeName
                            local.StackMemory.TypeName = typeName;

                            // Pointer
                            if (pointerChars.Length > 0 || arrayOrFunctionPointerChars.Length > 0)
                            {
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.POINTER;
                            }

                            // Array
                            if (dimensions.Length > 0)
                            {
                                local.StackMemory.TypeFlags |= EMemoryTypeFlags.ARRAY;
                            }

                            local.StackMemory.PointerLevel = (uint)pointerChars.Length;

                            {
                                Regex regex = new Regex(@"\((\*+)\)");
                                Match matchPointer = regex.Match(arrayOrFunctionPointerChars);

                                while (matchPointer.Success)
                                {
                                    uint size = (uint)matchPointer.Groups[1].Length;
                                    local.StackMemory.ArrayOrFunctionPointerLevels.Add(size);

                                    matchPointer = matchPointer.NextMatch();
                                }
                            }

                            {
                                Regex regex = new Regex(@"\[(\d+)\]");
                                Match matchDimeson = regex.Match(dimensions);

                                while (matchDimeson.Success)
                                {
                                    uint size = 0;
                                    Debug.Assert(uint.TryParse(matchDimeson.Groups[1].Value, out size));

                                    local.StackMemory.ArrayLengths.Add(size);

                                    matchDimeson = matchDimeson.NextMatch();
                                }
                            }

                            // name (fixed)
                            local.Name = variableName;
                        }
                    }
                    else
                    {
                        Regex rx = new Regex(@"\s{2}([0-9a-f]{8}).*\s(\w+)\s=\s");
                        Match match = rx.Match(line);

                        if (match.Success)
                        {
                            string stackAddr = match.Groups[1].Value;
                            string variableName = match.Groups[2].Value;
                            LocalVariable local = stackFrame.GetLocalVariable(variableName);

                            uint address = 0;
                            Debug.Assert(uint.TryParse(stackAddr, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out address));
                            local.StackMemory.Address = address;
                        }
                    }
                });
            }
            #endregion

            // Check if initialized
            if (!mCallStackOrNull.IsEmpty())
            {
                if (stackFrame.IsInitialized)
                {
                    goto UpdateMemory;
                }
            }

            #region Get Local Variable SizeOf
            {
                foreach (var name in stackFrame.LocalVariableNames)
                {
                    LocalVariable local = stackFrame.GetLocalVariable(name);

                    RequestInstruction(string.Format(CdbInstructionSet.EVALUATE_SIZEOF, name),
                        CdbInstructionSet.REQUEST_START_SIZEOF + ' ' + name, CdbInstructionSet.REQUEST_END_SIZEOF);
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

                        local.StackMemory.Size = size;

                        uint wordSize = size / 4 + (uint)(size % 4 > 0 ? 1 : 0);

                        Debug.Assert(local.StackMemory.ByteValues == null);
                        local.StackMemory.ByteValues = new byte[wordSize * 4];
                    });

                    string typeName = local.StackMemory.TypeName;
                    if (!mTypeSizeManagerOrNull.HasSize(typeName))
                    {
                        // Get Plain-Type Size
                        RequestInstruction(string.Format(CdbInstructionSet.EVALUATE_SIZEOF, typeName),
                            CdbInstructionSet.REQUEST_START_SIZEOF + ' ' + typeName, CdbInstructionSet.REQUEST_END_SIZEOF);
                        ReadResultLine(CdbInstructionSet.REQUEST_START_SIZEOF, CdbInstructionSet.REQUEST_END_SIZEOF, (string innerLine) =>
                        {
                            int lastIndex = innerLine.LastIndexOf(' ');
                            Debug.Assert(lastIndex >= 0);

                            string value = innerLine.Substring(lastIndex + 1);
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

                            mTypeSizeManagerOrNull.Add(typeName, size);
                        });
                    }
                }

                stackFrame.IsInitialized = true;
            }

            #endregion

UpdateMemory:

            #region Get Memory Words
            {
                foreach (var name in stackFrame.LocalVariableNames)
                {
                    LocalVariable local = stackFrame.GetLocalVariable(name);
                    RequestInstruction(string.Format(CdbInstructionSet.DISPLAY_MEMORY, local.StackMemory.ByteValues.Length / 4, "0x" + local.StackMemory.Address.ToString("X")),
                        CdbInstructionSet.REQUEST_START_DISPLAY_MEMORY + ' ' + name, CdbInstructionSet.REQUEST_END_DISPLAY_MEMORY);
                    ReadResultLine(CdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, CdbInstructionSet.REQUEST_END_DISPLAY_MEMORY, (string line) =>
                    {
                        local.StackMemory.SetValue(line);
                    });
                }
            }
            #endregion

            #region Get Heap Memory
            {
                foreach (var name in stackFrame.LocalVariableNames)
                {
                    LocalVariable local = stackFrame.GetLocalVariable(name);
                    if (local.StackMemory.TypeFlags.HasFlag(EMemoryTypeFlags.POINTER) && local.StackMemory.IsChanged)
                    {
                        byte[] byteValues = local.StackMemory.ByteValues;
                        uint wordValue = ((uint)byteValues[0] << 24) | ((uint)byteValues[1] << 16) | ((uint)byteValues[2] << 8) | (uint)byteValues[3];

                        RequestInstruction(string.Format(CdbInstructionSet.DISPLAY_HEAP, wordValue.ToString("X")),
                            CdbInstructionSet.REQUEST_START_HEAP + ' ' + name, CdbInstructionSet.REQUEST_END_HEAP);
                        ReadResultLine(CdbInstructionSet.REQUEST_START_HEAP, CdbInstructionSet.REQUEST_END_HEAP, (string line) =>
                        {
                            Regex rx = new Regex(@"^([0-9a-f]{8})\s\s([0-9a-f]{8})\s\s([0-9a-f]{8})\s\s([0-9a-f]{8})\s+([0-9a-f]+)\s+([0-9a-f]+)\s+([0-9a-f]+)\s+(busy)\s$");
                            Match match = rx.Match(line);

                            //Entry     User      Heap      Segment       Size  PrevSize  Unused    Flags
                            //-----------------------------------------------------------------------------
                            //00f3d0f0  00f3d0f8  00f30000  00f30000        18      1f20         c  busy 

                            if (match.Success)
                            {
                                //string entryAddr = match.Groups[1].Value;
                                string userAddr = match.Groups[2].Value;
                                //string heapAddr = match.Groups[3].Value;
                                //string segmentAddr = match.Groups[4].Value;
                                string sizeHex = match.Groups[5].Value;
                                //string prevSizeStr = match.Groups[6].Value;
                                string unusedHex = match.Groups[7].Value;

                                uint size = 0;
                                Debug.Assert(uint.TryParse(sizeHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out size));

                                uint unused = 0;
                                Debug.Assert(uint.TryParse(unusedHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out unused));

                                Debug.Assert(size >= unused);
                                uint used = size - unused;

                                // 포인터만이 heap을 가리킬 수 있다.
                                if (local.StackMemory.PointerLevel > 0)
                                {
                                    Debug.Assert(mTypeSizeManagerOrNull.HasSize(local.StackMemory.TypeName));

                                    uint unitSize = local.StackMemory.PointerLevel == 1 ? mTypeSizeManagerOrNull.GetSize(local.StackMemory.TypeName) : 4;
                                    Debug.Assert(used % unitSize == 0);
                                    uint length = used / unitSize;

                                    Debug.WriteLine("used memory: {0}, Length : {1}", used, length);
                                }
                            }
                        });
                    }
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
        }
    }
}
