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
                onLinePointerChanged();
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

        private HeapManager mHeapManagerOrNull;
        public HeapManager HeapManagerOrNull
        {
            get
            {
                return mHeapManagerOrNull;
            }
            set
            {
                mHeapManagerOrNull = value;
                OnPropertyChanged("HeapManagerOrNull");
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

        public void ExecuteGdb()
        {
            string dirPath = Path.GetDirectoryName(mSourcePathOrNull);
            string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);

            ProcessGdbOrNull = new Process();

            ProcessStartInfo processInfo = new ProcessStartInfo();
            processInfo.FileName = "gdb";
            processInfo.WorkingDirectory = dirPath;
            processInfo.Arguments = $"{fileNameOnly}.exe -q";
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
            PureTypeManager.Clear();
            HeapManagerOrNull = new HeapManager();

            #region set main breakpoint
            {
                RequestInstruction(GdbInstructionSet.DEFINE_COMMANDS,
                    null, null);
                RequestInstruction(GdbInstructionSet.SET_PAGINATION_OFF,
                    null, null);
                RequestInstruction(GdbInstructionSet.SET_UNWINDONSIGNAL_ON,
                    null, null);
                RequestInstruction(GdbInstructionSet.SET_BREAK_POINT_MAIN,
                    null, null);
                RequestInstruction(GdbInstructionSet.RUN,
                    GdbInstructionSet.REQUEST_START_GO_COMMAND, GdbInstructionSet.REQUEST_END_GO_COMMAND);
                ReadResultLine(GdbInstructionSet.REQUEST_START_GO_COMMAND, GdbInstructionSet.REQUEST_END_GO_COMMAND, ActionLinePointer);
                RequestInstruction(GdbInstructionSet.CLEAR_ALL_BREAK_POINTS,
                    null, null);
                RequestInstruction(GdbInstructionSet.CREATE_HEAPINFO,
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

                RequestInstruction(GdbInstructionSet.DISPLAY_HEAPINFO,
                    GdbInstructionSet.REQUEST_START_DISPLAY_HEAPINFO, GdbInstructionSet.REQUEST_END_DISPLAY_HEAPINFO);
                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_HEAPINFO, GdbInstructionSet.REQUEST_END_DISPLAY_HEAPINFO, actionAddHeap);
                HeapManagerOrNull.Update();
                HeapManagerOrNull.SetAllInvisible();
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
                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SYMBOLINFO, "0x" + functionWithOffsetAddressHex),
                        GdbInstructionSet.REQUEST_START_DISPLAY_SYMBOLINFO, GdbInstructionSet.REQUEST_END_DISPLAY_SYMBOLINFO);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SYMBOLINFO, GdbInstructionSet.REQUEST_END_DISPLAY_SYMBOLINFO, (string line) =>
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

            HeapManagerOrNull.Clear();
            RequestInstruction(GdbInstructionSet.DISPLAY_HEAPINFO,
                GdbInstructionSet.REQUEST_START_DISPLAY_HEAPINFO, GdbInstructionSet.REQUEST_END_DISPLAY_HEAPINFO);
            ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_HEAPINFO, GdbInstructionSet.REQUEST_END_DISPLAY_HEAPINFO, actionAddHeap);
            HeapManagerOrNull.Update();

            Queue<string> unregisteredPureTypeNames = new Queue<string>();

            #region Initialize StackFrames
            {
                for (int i = 0; i < mCallStackViewModel.CallStack.StackFrameKeys.Count; ++i)
                {
                    Models.StackFrame frame = mCallStackViewModel.CallStack.GetStackFrame(mCallStackViewModel.CallStack.StackFrameKeys[i]);
                    if (frame.IsInitialized)
                    {
                        continue;
                    }

                    // set stack frame for range compatiability
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

                    // examine local information by names
                    foreach (var name in frame.LocalVariableNames)
                    {
                        var local = frame.GetLocalVariable(name);

                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_ADDRESS, name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_ADDRESS, GdbInstructionSet.REQUEST_END_DISPLAY_ADDRESS);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_ADDRESS, GdbInstructionSet.REQUEST_END_DISPLAY_ADDRESS, (string line) =>
                        {
                            Regex regexLocalAddress = new Regex(@"\s0x([a-z0-9]+)");
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

                        // must be for primitive type
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

                        // TypeName Only
                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPENAME, name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                        {
                            string fullTypeName = line.Substring("type = ".Length);
                            local.StackMemory.TypeInfo.FullNameOrNull = fullTypeName;
                            if (!PureTypeManager.HasType(local.StackMemory.TypeInfo.PureName))
                            {
                                unregisteredPureTypeNames.Enqueue(local.StackMemory.TypeInfo.PureName);
                            }
                        });
                    }

                    frame.IsInitialized = true;
                }
            }
            #endregion

            #region Examine child member types of PureType
            while (unregisteredPureTypeNames.Count > 0)
            {
                string rootPureTypeName = unregisteredPureTypeNames.Dequeue();
                if (PureTypeManager.HasType(rootPureTypeName))
                {
                    continue;
                }

                var newRootPureType = new TypeInfo();
                newRootPureType.PureName = rootPureTypeName;
                PureTypeManager.AddType(rootPureTypeName, newRootPureType);

                // Pure Type Size
                RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, rootPureTypeName),
                    GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                {
                    int index = line.LastIndexOf(' ');
                    Debug.Assert(index > 0);

                    uint size = 0;
                    bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                    Debug.Assert(bSuccess);
                    newRootPureType.Size = size;
                });

                // Get Structure of Pure Type
                List<string> lines = new List<string>(64);
                RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPEINFO, rootPureTypeName),
                    GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                {
                    lines.Add(line);
                });

                // 최상위 타입
                {
                    Debug.Assert(lines.Count > 0);

                    Regex regexRootType = new Regex(@"(class|struct|enum|union)(.*)\s{");
                    Match matchRootType = regexRootType.Match(lines[0]);
                    if (matchRootType.Success)
                    {
                        switch (matchRootType.Groups[1].Value)
                        {
                            case "class":
                                newRootPureType.Flags |= EMemoryTypeFlags.CLASS;
                                break;

                            case "struct":
                                newRootPureType.Flags = EMemoryTypeFlags.STRUCT;
                                break;

                            case "enum":
                                newRootPureType.Flags = EMemoryTypeFlags.ENUM;
                                break;

                            case "union":
                                newRootPureType.Flags |= EMemoryTypeFlags.UNION;
                                break;

                            default:
                                Debug.Assert(false, "Invalid Block Type");
                                break;
                        }
                    }
                }

                Stack<TypeInfo> pureTypeStack = new Stack<TypeInfo>();
                pureTypeStack.Push(newRootPureType);

                for (int j = 1; j < lines.Count; ++j)
                {
                    string line = lines[j];
                    char lastChar = line[line.Length - 1];

                    Regex regexKind = new Regex(@"\/\*\s*((\d+)\s*\|){0,1}\s*(\d+)\s*\*\/(.*)");
                    Match matchOffsetAndSize = regexKind.Match(line);
                    if (matchOffsetAndSize.Success)
                    {
                        uint absoluteOffset = 0;
                        uint size = 0;

                        bool bSuccess = false;
                        if (matchOffsetAndSize.Groups[2].Value.Length == 0) // union
                        {
                            absoluteOffset = pureTypeStack.Peek().Offset;
                        }
                        else
                        {
                            bSuccess = uint.TryParse(matchOffsetAndSize.Groups[2].Value, out absoluteOffset);
                            Debug.Assert(bSuccess);
                        }
                        bSuccess = uint.TryParse(matchOffsetAndSize.Groups[3].Value, out size);
                        Debug.Assert(bSuccess);

                        if (lastChar == ';') // 변수
                        {
                            Regex regexMemberName = new Regex(@"[a-zA-Z_$][a-zA-Z_$0-9]*", RegexOptions.RightToLeft);
                            Match matchMemberName = regexMemberName.Match(matchOffsetAndSize.Groups[4].Value);

                            bool isValid = false;

                            Stack<TypeInfo> tempStack = new Stack<TypeInfo>(); // anonymous

                            while (matchMemberName.Success)
                            {
                                while (pureTypeStack.Count > 0 && pureTypeStack.Peek().PureName == null)
                                {
                                    var pop = pureTypeStack.Pop();
                                    tempStack.Push(pop);
                                }
                                RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_MEMBER_OFFSET, pureTypeStack.Peek().PureName, matchMemberName.Value),
                                    GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                                while (tempStack.Count > 0)
                                {
                                    var pop = tempStack.Pop();
                                    pureTypeStack.Push(pop);
                                }

                                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string innerLine) =>
                                {
                                    Regex regexRelativeOffset = new Regex(@"\s0x([a-z0-9]+)");
                                    Match matchRelativeOffset = regexRelativeOffset.Match(innerLine);
                                    if (matchRelativeOffset.Success)
                                    {
                                        string childRelativeOffsetHex = matchRelativeOffset.Groups[1].Value;

                                        uint childRelativeOffset = 0;
                                        bSuccess = uint.TryParse(childRelativeOffsetHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out childRelativeOffset);
                                        Debug.Assert(bSuccess);

                                        if (pureTypeStack.Peek().Flags.HasFlag(EMemoryTypeFlags.UNION))
                                        {
                                            isValid = (absoluteOffset == childRelativeOffset);
                                        }
                                        else
                                        {
                                            isValid = (absoluteOffset == pureTypeStack.Peek().Offset + childRelativeOffset);
                                        }
                                    }
                                });

                                if (isValid)
                                {
                                    break;
                                }

                                matchMemberName = matchMemberName.NextMatch();
                            }

                            Debug.Assert(isValid);

                            while (pureTypeStack.Count > 0 && pureTypeStack.Peek().PureName == null)
                            {
                                var pop = pureTypeStack.Pop();
                                tempStack.Push(pop);
                            }
                            RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_MEMBER_TYPE, pureTypeStack.Peek().PureName, matchMemberName.Value),
                                GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                            while (tempStack.Count > 0)
                            {
                                var pop = tempStack.Pop();
                                pureTypeStack.Push(pop);
                            }

                            ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string innerLine) =>
                            {
                                var newMember = new TypeInfo()
                                {
                                    MemberNameOrNull = matchMemberName.Value,
                                    Offset = absoluteOffset,
                                    Size = size
                                };
                                string fullName = innerLine.Substring("type = ".Length);
                                if (fullName == "enum {...}")
                                {
                                    fullName = "enum";
                                    newMember.Flags |= EMemoryTypeFlags.ENUM;
                                }
                                newMember.FullNameOrNull = fullName;
                                
                                pureTypeStack.Peek().Members.Add(newMember);

                                if (!newMember.Flags.HasFlag(EMemoryTypeFlags.ENUM) && !PureTypeManager.HasType(newMember.PureName))
                                {
                                    unregisteredPureTypeNames.Enqueue(newMember.PureName);
                                }
                            });
                        }
                        else
                        {
                            if (lastChar == '{')
                            {
                                Regex regexChildType = new Regex(@"(class|struct|enum|union)(.*)\s{");
                                Match matchChildType = regexChildType.Match(line);
                                if (matchChildType.Success)
                                {
                                    TypeInfo childBlockType = new TypeInfo()
                                    {
                                        Offset = absoluteOffset,
                                        Size = size,
                                    };

                                    switch (matchChildType.Groups[1].Value)
                                    {
                                        case "class":
                                            childBlockType.Flags |= EMemoryTypeFlags.CLASS;
                                            break;

                                        case "struct":
                                            childBlockType.Flags = EMemoryTypeFlags.STRUCT;
                                            break;

                                        case "enum":
                                            childBlockType.Flags = EMemoryTypeFlags.ENUM;
                                            break;

                                        case "union":
                                            childBlockType.Flags |= EMemoryTypeFlags.UNION;
                                            break;

                                        default:
                                            Debug.Assert(false, "Invalid Block Type");
                                            break;
                                    }

                                    string fullName = matchChildType.Groups[2].Value;

                                    int inheritanceSymbolIndex = fullName.IndexOf(" : ");
                                    if (inheritanceSymbolIndex >= 0)
                                    {
                                        fullName = fullName.Substring(0, inheritanceSymbolIndex);
                                    }
                                    if (fullName.Length > 0)
                                    {
                                        childBlockType.FullNameOrNull = fullName;
                                    }

                                    pureTypeStack.Push(childBlockType);
                                }
                            }
                        }
                    }
                    else
                    {
                        Regex regexNameOfBlock = new Regex(@"}\s(\w+);");
                        Match matchNameOfBlock = regexNameOfBlock.Match(line);
                        if (matchNameOfBlock.Success)
                        {
                            pureTypeStack.Peek().MemberNameOrNull = matchNameOfBlock.Groups[1].Value;
                        }

                        if (line.Contains('}')) // 닫는 문자 포함 시 스택에서 제거
                        {
                            var pop = pureTypeStack.Pop();
                            if (pureTypeStack.Count > 0)
                            {
                                pureTypeStack.Peek().Members.Add(pop);
                            }
                        }
                        // else => MemberName is null
                    }
                }
            }
            #endregion

            #region Update Locals of Values
            {
                StringBuilder memoryStringBuilder = new StringBuilder(8192);

                for (int i = 0; i < mCallStackViewModel.CallStack.StackFrames.Count; ++i)
                {
                    RequestInstruction(string.Format(GdbInstructionSet.SELECT_FRAME, i),
                        null, null);

                    Models.StackFrame frame = mCallStackViewModel.CallStack.StackFrames[i];

                    foreach (var local in frame.LocalVariables)
                    {
                        memoryStringBuilder.Clear();

                        uint wordCount = local.StackMemory.TypeInfo.Size / 4 + (uint)(local.StackMemory.TypeInfo.Size % 4 > 0 ? 1 : 0);

                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_MEMORY, wordCount, '&' + local.Name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY, (string line) =>
                        {
                            int index = line.IndexOf(':');
                            Debug.Assert(index != 6);

                            memoryStringBuilder.Append(line.Substring(index + 1));
                        });

                        local.StackMemory.SetValue(memoryStringBuilder);

                        if (local.StackMemory.TypeInfo.PointerLevel > 0)
                        {
                            uint totalLength = 1;
                            foreach (uint len in local.StackMemory.TypeInfo.ArrayLengths)
                            {
                                totalLength *= len;
                            }

                            for (uint j = 0; j < totalLength; ++j)
                            {
                                uint address = local.StackMemory.ByteValues[j * 4] +
                                    ((uint)local.StackMemory.ByteValues[j * 4 + 1] << 8) +
                                    ((uint)local.StackMemory.ByteValues[j * 4 + 2] << 16) +
                                    ((uint)local.StackMemory.ByteValues[j * 4 + 3] << 24);

                                var heapOrNull = HeapManagerOrNull.GetHeapOrNull(address);
                                if (heapOrNull != null)
                                {
                                    uint heapSize = heapOrNull.Size;

                                    heapOrNull.TypeInfo.Size = PureTypeManager.GetType(local.StackMemory.TypeInfo.PureName).Size;
                                    string fullName = local.StackMemory.TypeInfo.PureName;
                                    if (local.StackMemory.TypeInfo.PointerLevel >= 2)
                                    {
                                        heapOrNull.TypeInfo.Size = 4;
                                        fullName += ' ';
                                        fullName += new string('*', (int)local.StackMemory.TypeInfo.PointerLevel - 1);
                                    }
                                    heapOrNull.TypeInfo.FullNameOrNull = fullName;

                                    memoryStringBuilder.Clear();
                                    uint heapWordCount = heapSize / 4 + (heapSize % 4 > 0 ? 1u : 0);
                                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_MEMORY, heapWordCount, string.Format("0x{0:x8}", address)),
                                        GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY);
                                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY, (string line) =>
                                    {
                                        int index = line.IndexOf(':');
                                        Debug.Assert(index != 6);
                                        memoryStringBuilder.Append(line.Substring(index + 1));
                                    });
                                    heapOrNull.SetValue(memoryStringBuilder);
                                }
                            }
                        }
                    }
                }
            }
            #endregion
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

        private void actionAddHeap(string line)
        {
            ulong heapKey = 0;
            bool bSuccess = ulong.TryParse(line.Substring(0, 16), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out heapKey);
            Debug.Assert(bSuccess);

            bool bUsed = (line[16] == '1');
            if (bUsed)
            {
                HeapManagerOrNull.Add(heapKey);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler LinePointerChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void onLinePointerChanged()
        {
            LinePointerChanged?.Invoke(this, new PropertyChangedEventArgs("LinePointer"));
        }
    }
}
