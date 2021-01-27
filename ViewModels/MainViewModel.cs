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
using System.Collections.ObjectModel;

namespace CppMemoryVisualizer.ViewModels
{
    sealed class MainViewModel : INotifyPropertyChanged
    {
        public ICommand LoadSourceFileCommand { get; }
        public ICommand DebugCommand { get; }
        public ICommand GoCommand { get; }
        public ICommand StepOverCommand { get; }
        public ICommand StepInCommand { get; }
        public ICommand AddOrRemoveBreakPointCommand { get; }

        private Process mProcessGdbOrNull;
        public Process ProcessGdbOrNull
        {
            get
            {
                return mProcessGdbOrNull;
            }
            set
            {
                mProcessGdbOrNull = value;
                OnPropertyChanged("ProcessGdbOrNull");
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

        private BreakPointList mBreakPointList;
        public BreakPointList BreakPointList
        {
            get
            {
                return mBreakPointList;
            }
            set
            {
                mBreakPointList = value;
                OnPropertyChanged("BreakPointList");
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

        private CallStackViewModel mCallStackViewModel;
        public CallStackViewModel CallStackViewModel
        {
            get
            {
                return mCallStackViewModel;
            }
            set
            {
                mCallStackViewModel = value;
                OnPropertyChanged("CallStackViewModel");
            }
        }

        private PureTypeManager mPureTypeManagerOrNull;

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

        public void ExecuteGdb()
        {
            string dirPath = Path.GetDirectoryName(mSourcePathOrNull);
            string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);

            ProcessGdbOrNull = new Process();

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "gdb";
            processInfo.WorkingDirectory = dirPath;
            processInfo.Arguments = $"{fileNameOnly}.exe";
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardError = false;
            ProcessGdbOrNull.StartInfo = processInfo;

            Log = string.Empty;

            mProcessGdbOrNull.Start();

            CurrentInstruction = EDebugInstructionState.INITIALIZING;
            LinePointer = 0;
            CallStackViewModel = new CallStackViewModel();
            mPureTypeManagerOrNull = new PureTypeManager();

            #region set main breakpoint
            {
                RequestInstruction(GdbInstructionSet.SET_PAGINATION_OFF,
                    null, null);
                RequestInstruction(GdbInstructionSet.SET_BREAK_POINT_MAIN,
                    null, null);
                RequestInstruction(GdbInstructionSet.RUN,
                    GdbInstructionSet.REQUEST_START_INIT, GdbInstructionSet.REQUEST_END_INIT);
                ReadResultLine(GdbInstructionSet.REQUEST_START_INIT, GdbInstructionSet.REQUEST_END_INIT, ActionLinePointer);
                RequestInstruction(GdbInstructionSet.CLEAR_ALL_BREAK_POINTS,
                    null, null);

                if (BreakPointList.Count > 0)
                {
                    string fileName = Path.GetFileName(SourcePathOrNull);
                    for (int line = 1; line < BreakPointList.Indices.Count; ++line)
                    {
                        if (BreakPointList.Indices[line])
                        {
                            RequestInstruction(string.Format(GdbInstructionSet.ADD_BREAK_POINT, fileName, line),
                                null, null);
                        }
                    }
                }
            }
            #endregion

            UpdateGdb();

            CurrentInstruction = EDebugInstructionState.STANDBY;
        }

        public void ShutdownGdb()
        {
            if (mProcessGdbOrNull != null)
            {
                CurrentInstruction = EDebugInstructionState.DEAD;
                RequestInstruction(GdbInstructionSet.QUIT,
                    null, null);
                ProcessGdbOrNull = null;
            }
        }

        public void RequestInstruction(string instructionOrNull, string startOrNull, string endOrNull)
        {
            if (startOrNull != null)
            {
                mProcessGdbOrNull.StandardInput.WriteLine(string.Format(GdbInstructionSet.PRINTF, startOrNull));
            }
            if (instructionOrNull != null)
            {
                mProcessGdbOrNull.StandardInput.WriteLine(instructionOrNull);
            }
            if (endOrNull != null)
            {
                mProcessGdbOrNull.StandardInput.WriteLine(string.Format(GdbInstructionSet.PRINTF, endOrNull));
            }
        }

        public void ReadResultLine(string start, string end, Action<string> lambdaOrNull)
        {
            Debug.Assert(start != null);
            Debug.Assert(end != null);

            string line;

            do
            {
                line = mProcessGdbOrNull.StandardOutput.ReadLine();
                {
                    int lastIndex = line.LastIndexOf(GdbInstructionSet.OUTPUT_HEADER);
                    if (lastIndex != -1)
                    {
                        line = line.Substring(lastIndex + GdbInstructionSet.OUTPUT_HEADER.Length);
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
                line = mProcessGdbOrNull.StandardOutput.ReadLine();
                {
                    int lastIndex = line.LastIndexOf(GdbInstructionSet.OUTPUT_HEADER);
                    if (lastIndex != -1)
                    {
                        line = line.Substring(lastIndex + GdbInstructionSet.OUTPUT_HEADER.Length);
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

        public void UpdateGdb()
        {
            #region Get StackTrace
            {
                mCallStackViewModel.CallStack.Clear();

                uint frameCount = 0;
                RequestInstruction(GdbInstructionSet.DISPLAY_STACK_BACKTRACE,
                    GdbInstructionSet.REQUEST_START_DISPLAY_CALL_STACK, GdbInstructionSet.REQUEST_END_DISPLAY_CALL_STACK);
                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_CALL_STACK, GdbInstructionSet.REQUEST_END_DISPLAY_CALL_STACK, (string line) =>
                {
                    ++frameCount;
                });

                Regex regexFrameAddress = new Regex(@"^Stack frame at 0x([a-z0-9]+):$");
                Regex regexFunctionWithOffsetAddress = new Regex(@"^\seip\s=\s0x([a-z0-9]+)\sin\s");

                Regex regexFunctionSignature = new Regex(@"^((.*)\s\+\s(\d+)|(.*))\sin\ssection\s");

                for (uint i = 0; i < frameCount; ++i)
                {
                    string stackAddressHex = null;
                    string functionWithOffsetAddressHex = null;
                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_INFO_FRAME, i),
                        GdbInstructionSet.REQUEST_START_DISPLAY_INFO_FRAME, GdbInstructionSet.REQUEST_END_DISPLAY_INFO_FRAME);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_INFO_FRAME, GdbInstructionSet.REQUEST_END_DISPLAY_INFO_FRAME, (string line) =>
                    {
                        {
                            Match match = regexFrameAddress.Match(line);
                            if (match.Success)
                            {
                                stackAddressHex = match.Groups[1].Value;
                                return;
                            }
                        }

                        {
                            Match match = regexFunctionWithOffsetAddress.Match(line);
                            if (match.Success)
                            {
                                functionWithOffsetAddressHex = match.Groups[1].Value;
                            }
                        }
                    });

                    Debug.Assert(stackAddressHex != null);
                    Debug.Assert(functionWithOffsetAddressHex != null);

                    uint stackAddress = 0;
                    bool bSuccess = uint.TryParse(stackAddressHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out stackAddress);
                    Debug.Assert(bSuccess);

                    uint functionAddress = 0; // may have offset
                    bSuccess = uint.TryParse(functionWithOffsetAddressHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out functionAddress);
                    Debug.Assert(bSuccess);

                    string functionName = null;
                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_INFO_SYMBOL, "0x" + functionWithOffsetAddressHex),
                        GdbInstructionSet.REQUEST_START_DISPLAY_INFO_SYMBOL, GdbInstructionSet.REQUEST_END_DISPLAY_INFO_SYMBOL);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_INFO_SYMBOL, GdbInstructionSet.REQUEST_END_DISPLAY_INFO_SYMBOL, (string line) =>
                    {
                        Match match = regexFunctionSignature.Match(line);
                        if (match.Success)
                        {
                            // no offset
                            string signature = match.Groups[4].Value;
                            uint offset = 0;

                            // has offset
                            if (signature.Length == 0)
                            {
                                signature = match.Groups[2].Value;
                                bSuccess = uint.TryParse(match.Groups[3].Value, out offset);
                                Debug.Assert(bSuccess);
                                functionAddress -= offset;
                            }

                            int nameLen = signature.IndexOf('(');
                            if (nameLen > 0)
                            {
                                functionName = signature.Substring(0, nameLen);
                            }
                            else
                            {
                                functionName = signature;
                            }
                        }
                    });

                    Debug.Assert(functionName != null);

                    Debug.WriteLine("Stack addr: {0}, Function addr: {1}, name: {2}", stackAddress, functionAddress, functionName);
                    mCallStackViewModel.CallStack.Push(stackAddress, functionAddress, functionName);
                }
            }
            #endregion

            if (mCallStackViewModel.CallStack.IsEmpty())
            {
                LinePointer = 0;
                ShutdownGdb();

                return;
            }

            #region Get StackFrames
            {
                Regex regexLocalAddress = new Regex(@"\s0x([a-z0-9]+)");

                for (int i = 0; i < mCallStackViewModel.CallStack.Keys.Count; ++i)
                {
                    Models.StackFrame frame = mCallStackViewModel.CallStack.GetStackFrame(mCallStackViewModel.CallStack.Keys[i]);
                    Console.WriteLine("key = " + mCallStackViewModel.CallStack.Keys[i]);
                    if (frame.IsInitialized)
                    {
                        continue;
                    }

                    RequestInstruction(string.Format(GdbInstructionSet.SELECT_FRAME, i),
                        null, null);

                    // add argument names
                    RequestInstruction(GdbInstructionSet.DISPLAY_ARGUMENTS,
                        GdbInstructionSet.REQUEST_START_DISPLAY_ARGUMENTS, GdbInstructionSet.REQUEST_END_DISPLAY_ARGUMENTS);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_ARGUMENTS, GdbInstructionSet.REQUEST_END_DISPLAY_ARGUMENTS, (string line) =>
                    {
                        int index = line.IndexOf(" = ");
                        Debug.Assert(index > 0);

                        string name = line.Substring(0, index);
                        frame.TryAdd(name, true);
                    });

                    // add local names
                    RequestInstruction(GdbInstructionSet.DISPLAY_LOCAL_VARIABLES,
                        GdbInstructionSet.REQUEST_START_DISPLAY_LOCAL_VARIABLES, GdbInstructionSet.REQUEST_END_DISPLAY_LOCAL_VARIABLES);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_LOCAL_VARIABLES, GdbInstructionSet.REQUEST_END_DISPLAY_LOCAL_VARIABLES, (string line) =>
                    {
                        int index = line.IndexOf(" = ");
                        Debug.Assert(index > 0);

                        string name = line.Substring(0, index);
                        frame.TryAdd(name, false);
                    });

                    foreach (var name in frame.LocalVariableNames)
                    {
                        var local = frame.GetLocalVariable(name);

                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_ADDRESS, name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_ADDRESS, GdbInstructionSet.REQUEST_END_DISPLAY_ADDRESS);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_ADDRESS, GdbInstructionSet.REQUEST_END_DISPLAY_ADDRESS, (string line) =>
                        {
                            Match match = regexLocalAddress.Match(line);
                            if (match.Success)
                            {
                                string addressHex = match.Groups[1].Value;
                                uint address = 0;
                                bool bSuccess = uint.TryParse(addressHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out address);
                                Debug.Assert(bSuccess);
                                local.StackMemory.Address = address;
                            }
                        });

                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                        {
                            int index = line.LastIndexOf(' ');
                            Debug.Assert(index > 0);

                            uint size = 0;
                            bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                            Debug.Assert(bSuccess);
                            local.StackMemory.TypeInfo.Size = size;
                            
                            uint wordCount = size / 4 + (uint)(size % 4 > 0 ? 1 : 0);
                            Debug.Assert(local.StackMemory.ByteValues == null);
                            local.StackMemory.ByteValues = new byte[wordCount * 4];
                        });

                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPE_NAME, name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                        {
                            string fullTypeName = line.Substring("type = ".Length);
                            local.StackMemory.TypeInfo.FullName = fullTypeName;
                        });

                        string pureTypeName = local.StackMemory.TypeInfo.PureName;
                        if (!mPureTypeManagerOrNull.HasType(pureTypeName))
                        {
                            var newPureType = new TypeInfo();

                            newPureType.PureName = pureTypeName;

                            // Pure Type Size
                            RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, pureTypeName),
                                GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                            ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                            {
                                int index = line.LastIndexOf(' ');
                                Debug.Assert(index > 0);

                                uint size = 0;
                                bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                                Debug.Assert(bSuccess);
                                newPureType.Size = size;
                            });

                            int depth = 0;
                            // Pure Type Info
                            RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPE_INFO, pureTypeName),
                                GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                            ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                            {
                                if (depth == 0)
                                {
                                    Regex regexKind = new Regex(@"^type\s=(\sclass|\sstruct|\senum|\sunion){0,1}\s.*\s{$");
                                    Match matchKind = regexKind.Match(line);
                                    if (matchKind.Success)
                                    {
                                        string t = matchKind.Groups[1].Value;
                                        if ("class" == t)
                                        {
                                            newPureType.Flags |= EMemoryTypeFlags.CLASS;
                                        }
                                        else if ("struct" == t)
                                        {
                                            newPureType.Flags |= EMemoryTypeFlags.STRUCT;
                                        }
                                        else if ("enum" == t)
                                        {
                                            newPureType.Flags |= EMemoryTypeFlags.ENUM;
                                        }
                                        else if ("union" == t)
                                        {
                                            newPureType.Flags |= EMemoryTypeFlags.UNION;
                                        }
                                    }
                                }
                                else if (depth == 1)
                                {
                                    Regex regexOffsetAndSize = new Regex(@"^\/\*\s*(\d+)\s*\|\s*(\d+)\s*\*\/");
                                    Match matchOffsetAndSize = regexOffsetAndSize.Match(line);
                                    if (matchOffsetAndSize.Success)
                                    {
                                        var memberType = new TypeInfo();

                                        uint offset = 0;
                                        uint size = 0;

                                        bool bSuccess = uint.TryParse(matchOffsetAndSize.Groups[1].Value, out offset);
                                        Debug.Assert(bSuccess);

                                        bSuccess = uint.TryParse(matchOffsetAndSize.Groups[2].Value, out size);
                                        Debug.Assert(bSuccess);

                                        Regex regexMemberName = new Regex(@"[a-zA-Z_$][a-zA-Z_$0-9]*", RegexOptions.RightToLeft);
                                        Match matchMemberName = regexMemberName.Match(line);

                                        if (matchMemberName.Success)
                                        {
                                            memberType.MemberNameOrNull = matchMemberName.Value;
                                        }
                                        memberType.Offset = offset;
                                        memberType.Size = size;

                                        newPureType.Members.Add(memberType);
                                    }
                                }

                                if (line.Contains('{'))
                                {
                                    ++depth;
                                }
                                else if (line.Contains('}'))
                                {
                                    --depth;
                                }

                                if (depth == 1 && line.Contains('}'))
                                {
                                    Regex regexMemberName = new Regex(@"}\s(\w+);$");
                                    Match matchMemberName = regexMemberName.Match(line);

                                    if (matchMemberName.Success)
                                    {
                                        var memberType = newPureType.Members[newPureType.Members.Count - 1];
                                        memberType.MemberNameOrNull = matchMemberName.Groups[1].Value;
                                    }
                                }
                            });

                            // members' typename
                            for (int n = 0; n < newPureType.Members.Count; ++n)
                            {
                                var memberType = newPureType.Members[n];
                                if (memberType.MemberNameOrNull != null)
                                {
                                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPE_NAME, newPureType.PureName + "::" + memberType.MemberNameOrNull),
                                        GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                                    {
                                        string fullTypeName = line.Substring("type = ".Length);
                                        memberType.FullName = fullTypeName;
                                    });
                                }
                            }

                            mPureTypeManagerOrNull.AddType(pureTypeName, newPureType);
                        }
                    }

                    frame.IsInitialized = true;
                }
            }
            #endregion

            {
                StringBuilder memoryWords = new StringBuilder(8192);

                for (int i = 0; i < mCallStackViewModel.CallStack.Stack.Count; ++i)
                {
                    RequestInstruction(string.Format(GdbInstructionSet.SELECT_FRAME, i),
                        null, null);

                    Models.StackFrame frame = mCallStackViewModel.CallStack.Stack[i];

                    foreach (var local in frame.LocalVariables)
                    {
                        memoryWords.Clear();

                        uint wordCount = local.StackMemory.TypeInfo.Size / 4 + (uint)(local.StackMemory.TypeInfo.Size % 4 > 0 ? 1 : 0);

                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_MEMORY, wordCount, '&' + local.Name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY, (string line) =>
                        {
                            int index = line.IndexOf(':');
                            Debug.Assert(index != 6);

                            memoryWords.Append(line.Substring(index + 1));
                        });

                        local.StackMemory.SetValue(memoryWords);
                    }
                }
            }
        }

        public void ActionLinePointer(string line)
        {
            Regex rx = new Regex(@"^(\d+)\t(.*)");
            Match match = rx.Match(line);

            if (match.Success)
            {
                Debug.WriteLine("Line {0}: `{1}`", match.Groups[1].Value, match.Groups[2].Value);

                uint lineNumber = 0;
                bool bSuccess = uint.TryParse(match.Groups[1].Value, out lineNumber);
                Debug.Assert(bSuccess);
                Debug.Assert(lineNumber > 0);
                LinePointer = lineNumber;
            }
        }

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
    }
}
