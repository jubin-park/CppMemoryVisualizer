using CppMemoryVisualizer.Commands;
using CppMemoryVisualizer.Constants;
using CppMemoryVisualizer.Enums;
using CppMemoryVisualizer.Models;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace CppMemoryVisualizer.ViewModels
{
    sealed class MainViewModel : INotifyPropertyChanged
    {
        #region ICommands
        public ICommand LoadSourceFileCommand { get; }
        public ICommand DebugCommand { get; }
        public ICommand GoCommand { get; }
        public ICommand StepOverCommand { get; }
        public ICommand StepInCommand { get; }
        public ICommand AddOrRemoveBreakPointCommand { get; }
        public ICommand MemorySegmentAddressClickCommand { get; }
        public ICommand MemorySegmentPointerValueClickCommand { get; }
        #endregion

        #region Properties
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
                onPropertyChanged("ProcessGdbOrNull");
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
                onPropertyChanged("CurrentInstruction");
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
                onPropertyChanged("StandardCppVersion");
            }
        }

        private string mLog = string.Empty;
        public string Log
        {
            get
            {
                return mLog;
            }
            set
            {
                mLog = value;
                onPropertyChanged("Log");
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
                onPropertyChanged("SourcePathOrNull");
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
                onPropertyChanged("SourceCode");
            }
        }

        private BreakPointList mBreakPointListOrNull;
        public BreakPointList BreakPointListOrNull
        {
            get
            {
                return mBreakPointListOrNull;
            }
            set
            {
                mBreakPointListOrNull = value;
                onPropertyChanged("BreakPointListOrNull");
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
                onPropertyChanged("LinePointer");
                onLinePointerChanged();
            }
        }

        private CallStack mCallStackOrNull;
        public CallStack CallStackOrNull
        {
            get
            {
                return mCallStackOrNull;
            }
            set
            {
                mCallStackOrNull = value;
                onPropertyChanged("CallStackOrNull");
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
                onPropertyChanged("HeapManagerOrNull");
            }
        }

        private uint[] mCapturedCounts = new uint[4];
        public uint CapturedStackMemoryAddressCount
        {
            get
            {
                return mCapturedCounts[0];
            }
            set
            {
                mCapturedCounts[0] = value;
                onPropertyChanged("CapturedStackMemoryAddressCount");
            }
        }

        public uint CapturedStackMemoryPointerValueCount
        {
            get
            {
                return mCapturedCounts[1];
            }
            set
            {
                mCapturedCounts[1] = value;
                onPropertyChanged("CapturedStackMemoryPointerValueCount");
            }
        }

        public uint CapturedHeapMemoryAddressCount
        {
            get
            {
                return mCapturedCounts[2];
            }
            set
            {
                mCapturedCounts[2] = value;
                onPropertyChanged("CapturedHeapMemoryAddressCount");
            }
        }

        public uint CapturedHeapMemoryPointerValueCount
        {
            get
            {
                return mCapturedCounts[3];
            }
            set
            {
                mCapturedCounts[3] = value;
                onPropertyChanged("CapturedHeapMemoryPointerValueCount");
            }
        }
        #endregion

        public MainViewModel()
        {
            LoadSourceFileCommand = new LoadSourceFileCommand(this);
            DebugCommand = new DebugCommand(this);
            GoCommand = new GoCommand(this);
            StepOverCommand = new StepOverCommand(this);
            StepInCommand = new StepInCommand(this);
            AddOrRemoveBreakPointCommand = new AddOrRemoveBreakPointCommand(this);
            MemorySegmentAddressClickCommand = new MemorySegmentAddressClickCommand(this);
            MemorySegmentViewModel.AddressClickCommand = (MemorySegmentAddressClickCommand)MemorySegmentAddressClickCommand;
            MemorySegmentPointerValueClickCommand = new MemorySegmentPointerValueClickCommand(this);
            MemorySegmentViewModel.PointerValueClickCommand = (MemorySegmentPointerValueClickCommand)MemorySegmentPointerValueClickCommand;
        }

        public void ExecuteGdb()
        {
            Debug.Assert(null != mSourcePathOrNull);
            Debug.Assert(mSourcePathOrNull.Length > 0);

            string dirPath = Path.GetDirectoryName(mSourcePathOrNull);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mSourcePathOrNull);

            ShutdownGdb();
            ProcessGdbOrNull = new Process() { 
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "gdb",
                    WorkingDirectory = dirPath,
                    Arguments = $"{fileNameWithoutExtension}.exe -q --interpreter=mi",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = false
                }
            };
            ProcessGdbOrNull.Start();

            Log = string.Empty;
            CurrentInstruction = EDebugInstructionState.INITIALIZING;
            CallStackOrNull = new CallStack();
            HeapManagerOrNull = new HeapManager();
            PureTypeManager.Clear();
            // keep BreakPointListOrNull because file is same

            #region set main breakpoint
            {
                RequestInstruction(GdbInstructionSet.DEFINE_COMMANDS, null, null);
                RequestInstruction(GdbInstructionSet.SET_PAGINATION_OFF, null, null);
                RequestInstruction(GdbInstructionSet.SET_UNWINDONSIGNAL_ON, null, null);
                RequestInstruction(GdbInstructionSet.SET_BREAK_POINT_MAIN, null, null);
                RequestInstruction(GdbInstructionSet.RUN,
                    GdbInstructionSet.REQUEST_START_GO_COMMAND, GdbInstructionSet.REQUEST_END_GO_COMMAND);
                ReadResultLine(GdbInstructionSet.REQUEST_START_GO_COMMAND, GdbInstructionSet.REQUEST_END_GO_COMMAND, ActionLinePointer);
                RequestInstruction(GdbInstructionSet.CLEAR_ALL_BREAK_POINTS, null, null);
                RequestInstruction(GdbInstructionSet.CREATE_HEAPINFO, null, null);

                // re-mark breakpoints
                if (BreakPointListOrNull.Count > 0)
                {
                    string fileName = Path.GetFileName(SourcePathOrNull);
                    for (int line = 1; line < BreakPointListOrNull.Indices.Count; ++line)
                    {
                        if (BreakPointListOrNull.Indices[line])
                        {
                            RequestInstruction(string.Format(GdbInstructionSet.ADD_BREAK_POINT, fileName, line), null, null);
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
            if (null != ProcessGdbOrNull && !ProcessGdbOrNull.HasExited)
            {
                CurrentInstruction = EDebugInstructionState.DEAD;
                RequestInstruction(GdbInstructionSet.QUIT, null, null);

                ProcessGdbOrNull.Close();
                ProcessGdbOrNull = null;
            }
        }

        public void UpdateGdb()
        {
            #region Get StackTrace
            {
                CallStackOrNull.Clear();

                Debug.Assert(null != mSourcePathOrNull);

                uint frameCount = 0;
                bool isMainExisted = false;
                RequestInstruction(GdbInstructionSet.DISPLAY_STACK_BACKTRACE,
                    GdbInstructionSet.REQUEST_START_DISPLAY_CALL_STACK, GdbInstructionSet.REQUEST_END_DISPLAY_CALL_STACK);
                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_CALL_STACK, GdbInstructionSet.REQUEST_END_DISPLAY_CALL_STACK, (string line) =>
                {
                    ++frameCount;
                    if (line.Contains("main (")) // "#0  main () at Untitled1.cpp:9"
                    {
                        isMainExisted = true;
                    }
                });

                if (!isMainExisted || 0 == frameCount)
                {
                    LinePointer = 0;
                    ShutdownGdb();

                    return;
                }

                for (uint i = 0; i < frameCount; ++i)
                {
                    string stackFrameAddressHex = null;
                    string functionAddressHex = null;

                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_INFO_FRAME, i),
                        GdbInstructionSet.REQUEST_START_DISPLAY_INFO_FRAME, GdbInstructionSet.REQUEST_END_DISPLAY_INFO_FRAME);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_INFO_FRAME, GdbInstructionSet.REQUEST_END_DISPLAY_INFO_FRAME, (string line) =>
                    {
                        if (null == stackFrameAddressHex)
                        {
                            // "Stack frame at 0x63fee0:"
                            Match match = RegexSet.REGEX_FRAME_ADDRESS.Match(line);
                            if (match.Success)
                            {
                                stackFrameAddressHex = match.Groups[1].Value;
                            }
                        }
                        else if (null == functionAddressHex)
                        {
                            // " eip = 0x4015e7 in main (Untitled1.cpp:9); saved eip = 0x401396"
                            Match match = RegexSet.REGEX_FUNCTION_ADDRESS.Match(line);
                            if (match.Success)
                            {
                                functionAddressHex = match.Groups[1].Value;
                            }
                        }
                    });
                    
                    uint stackFrameAddress = 0;
                    {
                        Debug.Assert(null != stackFrameAddressHex);
                        bool bSuccess = uint.TryParse(stackFrameAddressHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out stackFrameAddress);
                        Debug.Assert(bSuccess);
                    }

                    uint functionAddress = 0;
                    {
                        Debug.Assert(null != functionAddressHex);
                        bool bSuccess = uint.TryParse(functionAddressHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out functionAddress);
                        Debug.Assert(bSuccess);
                    }

                    string functionName = null;
                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SYMBOLINFO, "0x" + functionAddressHex),
                        GdbInstructionSet.REQUEST_START_DISPLAY_SYMBOLINFO, GdbInstructionSet.REQUEST_END_DISPLAY_SYMBOLINFO);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SYMBOLINFO, GdbInstructionSet.REQUEST_END_DISPLAY_SYMBOLINFO, (string line) =>
                    {
                        if (null == functionName)
                        {
                            // "main + 23 in section .text of C:\\Temp\\Untitled1.exe"
                            Match match = RegexSet.REGEX_FUNCTION_SIGNATURE.Match(line);
                            if (match.Success)
                            {
                                // try no offset first
                                string signature = match.Groups[4].Value;

                                // when function has offset
                                if (signature.Length == 0)
                                {
                                    uint offset = 0;
                                    signature = match.Groups[2].Value;
                                    bool bSuccess = uint.TryParse(match.Groups[3].Value, out offset);
                                    Debug.Assert(bSuccess);
                                    functionAddress -= offset;
                                }

                                // if function has arguments
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
                        }
                    });
                    Debug.Assert(functionName != null);

                    Debug.WriteLine("Stack addr: {0}, Function addr: {1}, name: {2}", stackFrameAddress, functionAddress, functionName);
                    CallStackOrNull.Push(stackFrameAddress, functionAddress, functionName);
                }
            }
            #endregion

            #region Update Heap memories
            {
                HeapManagerOrNull.Clear();
                RequestInstruction(GdbInstructionSet.DISPLAY_HEAPINFO,
                    GdbInstructionSet.REQUEST_START_DISPLAY_HEAPINFO, GdbInstructionSet.REQUEST_END_DISPLAY_HEAPINFO);
                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_HEAPINFO, GdbInstructionSet.REQUEST_END_DISPLAY_HEAPINFO, actionAddHeap);
                HeapManagerOrNull.Update();

                StringBuilder memoryStringBuilder = new StringBuilder(1024);
                
                foreach (var heap in mHeapManagerOrNull.Heaps)
                {
                    heap.TypeInfo = null;

                    uint heapWordCount = heap.Size / 4 + (heap.Size % 4 > 0 ? 1u : 0);
                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_MEMORY, heapWordCount, string.Format("0x{0:x8}", heap.Address)),
                        GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY, (string line) =>
                    {
                        // "0x10892f0:\t0x4e494d45\t0x35204d45\t0x4e454330\t0x494c2054"
                        int index = line.IndexOf(':');
                        Debug.Assert(index > 0);
                        memoryStringBuilder.Append(line.Substring(index + 1));
                    });
                    heap.SetValue(memoryStringBuilder);

                    memoryStringBuilder.Clear();
                }
            }
            #endregion

            Queue<string> unregisteredPureTypeNames = new Queue<string>();

            #region Initialize StackFrames
            {
                for (int i = 0; i < CallStackOrNull.StackFrameKeys.Count; ++i)
                {
                    // set stack frame for range compatiability
                    RequestInstruction(string.Format(GdbInstructionSet.SELECT_FRAME, i), null, null);

                    Models.StackFrame frame = CallStackOrNull.GetStackFrame(CallStackOrNull.StackFrameKeys[i]);
                    frame.Clear();

                    // add arguments
                    RequestInstruction(GdbInstructionSet.DISPLAY_ARGUMENTS,
                        GdbInstructionSet.REQUEST_START_DISPLAY_ARGUMENTS, GdbInstructionSet.REQUEST_END_DISPLAY_ARGUMENTS);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_ARGUMENTS, GdbInstructionSet.REQUEST_END_DISPLAY_ARGUMENTS, (string line) =>
                    {
                        // "obj = @0x62feb7: {<No data fields>}"
                        int index = line.IndexOf(" = ");
                        Debug.Assert(index > 0);

                        frame.AddArgumentLocalVariable(line.Substring(0, index));
                    });

                    // add locals
                    RequestInstruction(GdbInstructionSet.DISPLAY_LOCAL_VARIABLES,
                        GdbInstructionSet.REQUEST_START_DISPLAY_LOCAL_VARIABLES, GdbInstructionSet.REQUEST_END_DISPLAY_LOCAL_VARIABLES);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_LOCAL_VARIABLES, GdbInstructionSet.REQUEST_END_DISPLAY_LOCAL_VARIABLES, (string line) =>
                    {
                        // "p1 = 0xffff"
                        int index = line.IndexOf(" = ");
                        Debug.Assert(index > 0);

                        frame.AddLocalVariable(line.Substring(0, index));
                    });

                    // examine locals
                    foreach (var local in frame.LocalVariables)
                    {
                        // get address of local
                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_ADDRESS, local.Name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_ADDRESS, GdbInstructionSet.REQUEST_END_DISPLAY_ADDRESS);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_ADDRESS, GdbInstructionSet.REQUEST_END_DISPLAY_ADDRESS, (string line) =>
                        {
                            // "$1 = (std::string *) 0x63fea4"
                            Match match = RegexSet.REGEX_LOCAL_ADDRESS.Match(line);
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
                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, local.Name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                        {
                            // "$2 = 24"
                            int index = line.LastIndexOf(' ');
                            Debug.Assert(index > 0);

                            uint size = 0;
                            bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                            Debug.Assert(bSuccess);
                            local.StackMemory.TypeInfo.Size = size;
                            
                            uint wordCount = size / 4 + (uint)(size % 4 > 0 ? 1 : 0);
                            Debug.Assert(null == local.StackMemory.ByteValues);
                            local.StackMemory.ByteValues = new byte[wordCount * 4];
                        });

                        // TypeName Only
                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPENAME, local.Name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                        {
                            // "type = std::string"
                            local.StackMemory.TypeInfo.FullNameOrNull = line.Substring("type = ".Length);
                            if (!PureTypeManager.HasType(local.StackMemory.TypeInfo.PureName))
                            {
                                unregisteredPureTypeNames.Enqueue(local.StackMemory.TypeInfo.PureName);
                            }
                        });
                    }
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

                    Regex regexRootType = new Regex(@"(class|struct|enum|union).*\s{");
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
                    else
                    {
                        string fullTypeName = lines[0].Substring("type = ".Length);
                        newRootPureType.FullNameOrNull = fullTypeName;
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

                            string fullName = null;
                            ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string innerLine) =>
                            {
                                fullName = innerLine.Substring("type = ".Length);
                            });
                            Debug.Assert(fullName != null);

                            { // 값 타입만 원시 타입으로 변환
                                List<string> infos = new List<string>();
                                RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPEINFO, fullName),
                                    GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string innerLine) =>
                                {
                                    infos.Add(innerLine);
                                });
                                
                                if (infos.Count == 1)
                                {
                                    fullName = infos[0].Substring("type = ".Length);
                                }
                            }

                            var newMember = new TypeInfo()
                            {
                                MemberNameOrNull = matchMemberName.Value,
                                Offset = absoluteOffset,
                                Size = size
                            };

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
                        }
                        else
                        {
                            if (lastChar == '{')
                            {
                                Regex regexChildType = new Regex(@"(class|struct|enum|union)\s({|(.*)\s{)");
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

                                    string fullName = matchChildType.Groups[3].Value;

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

                for (int i = 0; i < CallStackOrNull.StackFrames.Count; ++i)
                {
                    RequestInstruction(string.Format(GdbInstructionSet.SELECT_FRAME, i),
                        null, null);

                    Models.StackFrame frame = CallStackOrNull.StackFrames[i];

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

                        var stackTypes = new Stack<StackKey>();
                        {
                            uint totalLength = local.StackMemory.TypeInfo.GetTotalLength();
                            uint sizePerSegment = local.StackMemory.TypeInfo.Size / totalLength;

                            if (local.StackMemory.TypeInfo.PointerLevel == 0 && local.StackMemory.TypeInfo.ArrayOrFunctionPointerLevels.Count == 0)
                            {
                                local.StackMemory.TypeInfo.Members = PureTypeManager.GetType(local.StackMemory.TypeInfo.PureName).Members;
                            }

                            for (uint j = 0; j < totalLength; ++j)
                            {
                                stackTypes.Push(new StackKey()
                                {
                                    Type = local.StackMemory.TypeInfo.GetElementOfArray(),
                                    StartOffset = j * sizePerSegment
                                });
                            }
                        }

                        {
                            while (stackTypes.Count > 0)
                            {
                                StackKey pop = stackTypes.Pop();

                                uint totalLength = pop.Type.GetTotalLength();
                                uint sizePerSegment = pop.Type.Size / totalLength;

                                if (pop.Type.PointerLevel == 0 && pop.Type.ArrayOrFunctionPointerLevels.Count == 0)
                                {
                                    for (uint j = 0; j < totalLength; ++j)
                                    {
                                        foreach (TypeInfo member in pop.Type.Members)
                                        {
                                            stackTypes.Push(new StackKey
                                            {
                                                Type = member,
                                                StartOffset = pop.StartOffset + j * totalLength
                                            });
                                        }
                                    }
                                }
                                else if (pop.Type.PointerLevel > 0)
                                {
                                    for (uint j = 0; j < totalLength; ++j)
                                    {
                                        uint offset = pop.StartOffset + pop.Type.Offset + j * 4;
                                        uint address = local.StackMemory.ByteValues[offset] +
                                            ((uint)local.StackMemory.ByteValues[offset + 1] << 8) +
                                            ((uint)local.StackMemory.ByteValues[offset + 2] << 16) +
                                            ((uint)local.StackMemory.ByteValues[offset + 3] << 24);

                                        HeapMemoryInfo heapOrNull = HeapManagerOrNull.GetHeapOrNull(address);
                                        if (heapOrNull != null)
                                        {
                                            heapOrNull.TypeInfo = pop.Type.GetDereference();
                                        }
                                    }
                                }
                            }
                        }

                        local.UpdateMemorySegments();
                    }
                }
            }
            #endregion

            #region Determine heap type from another's
            {
                var stackTypes = new Stack<StackKey>();
                foreach (var heap in mHeapManagerOrNull.Heaps)
                {
                    if (heap.TypeInfo == null)
                    {
                        continue;

                    }

                    {
                        uint sizePerSegment = heap.TypeInfo.Size;
                        uint totalLength = heap.Size / sizePerSegment;

                        for (uint i = 0; i < totalLength; ++i)
                        {
                            stackTypes.Push(new StackKey()
                            {
                                Type = heap.TypeInfo.GetElementOfArray(),
                                StartOffset = i * sizePerSegment
                            });
                        }
                    }

                    while (stackTypes.Count > 0)
                    {
                        StackKey pop = stackTypes.Pop();

                        uint totalLength = pop.Type.GetTotalLength();
                        uint sizePerSegment = pop.Type.Size / totalLength;

                        if (pop.Type.PointerLevel == 0 && pop.Type.ArrayOrFunctionPointerLevels.Count == 0)
                        {
                            for (uint i = 0; i < totalLength; ++i)
                            {
                                foreach (TypeInfo member in pop.Type.Members)
                                {
                                    stackTypes.Push(new StackKey
                                    {
                                        Type = member,
                                        StartOffset = pop.StartOffset + i * totalLength
                                    });
                                }
                            }
                        }
                        else if (pop.Type.PointerLevel > 0)
                        {
                            for (uint i = 0; i < totalLength; ++i)
                            {
                                uint offset = pop.StartOffset + pop.Type.Offset + i * 4;
                                uint address = heap.ByteValues[offset] +
                                    ((uint)heap.ByteValues[offset + 1] << 8) +
                                    ((uint)heap.ByteValues[offset + 2] << 16) +
                                    ((uint)heap.ByteValues[offset + 3] << 24);

                                HeapMemoryInfo anotherHeapOrNull = HeapManagerOrNull.GetHeapOrNull(address);
                                if (anotherHeapOrNull != null)
                                {
                                    anotherHeapOrNull.TypeInfo = pop.Type.GetDereference();
                                }
                            }
                        }
                    }

                    heap.UpdateMemorySegements();
                }
            }
            #endregion

            ClearCapturedCounts();
        }

        public void ClearCapturedCounts()
        {
            CapturedHeapMemoryAddressCount = 0;
            CapturedHeapMemoryPointerValueCount = 0;
            CapturedStackMemoryAddressCount = 0;
            CapturedStackMemoryPointerValueCount = 0;
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
#if GDBLOG
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
#if GDBLOG
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

        public void ActionLinePointer(string line)
        {
            Debug.Assert(null != line);

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
            Debug.Assert(null != line);
            Debug.Assert(17 == line.Length);

            ulong heapKey;
            bool bSuccess = ulong.TryParse(line.Substring(0, 16), NumberStyles.HexNumber, CultureInfo.CurrentCulture, out heapKey);
            Debug.Assert(bSuccess);

            if ('1' == line[16]) // is heap memory used
            {
                HeapManagerOrNull.Add(heapKey);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler LinePointerChanged;

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void onLinePointerChanged()
        {
            LinePointerChanged?.Invoke(this, new PropertyChangedEventArgs("LinePointer"));
        }

        sealed class StackKey
        {
            public TypeInfo Type;
            public uint StartOffset;
        }
    }
}
