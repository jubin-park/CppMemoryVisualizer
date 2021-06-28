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
using System.Windows;

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

        private double mStackMemoryViewerZoom = 1.0;
        public double StackMemoryViewerZoom
        {
            get
            {
                return mStackMemoryViewerZoom;
            }
            set
            {
                mStackMemoryViewerZoom = value;
                onPropertyChanged("StackMemoryViewerZoom");
            }
        }

        private double mHeapMemoryViewerZoom = 1.0;
        public double HeapMemoryViewerZoom
        {
            get
            {
                return mHeapMemoryViewerZoom;
            }
            set
            {
                mHeapMemoryViewerZoom = value;
                onPropertyChanged("HeapMemoryViewerZoom");
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

        public bool ExecuteGdb()
        {
            Debug.Assert(null != mSourcePathOrNull);
            Debug.Assert(mSourcePathOrNull.Length > 0);

            string dirPath = Path.GetDirectoryName(mSourcePathOrNull);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mSourcePathOrNull);

            if (!File.Exists(Path.Combine(dirPath, fileNameWithoutExtension + ".exe")))
            {
                MessageBox.Show($"{fileNameWithoutExtension}.exe 파일이 존재하지 않습니다. 컴파일 에러가 발생하지 않았는지 확인하십시오.", App.WINDOW_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            ProcessGdbOrNull = new Process() { 
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "gdb",
                    WorkingDirectory = dirPath,
                    Arguments = $"{fileNameWithoutExtension}.exe -q",
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

            #region check if 32-bit
            {
                bool is32Bits = false;
                // must be for primitive type
                RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, "void*"),
                    GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                {
                    // "$2 = 24"
                    int index = line.LastIndexOf(' ');
                    Debug.Assert(index > 0);

                    uint size = 0;
                    bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                    Debug.Assert(bSuccess);
                    is32Bits = (size == TypeInfo.POINTER_SIZE);
                });

                if (!is32Bits)
                {
                    MessageBox.Show("msys2 32비트 버전에서만 실행 가능합니다.", App.WINDOW_TITLE, MessageBoxButton.OK, MessageBoxImage.Error);

                    return false;
                }
            }
            #endregion

            #region initialize gdb
            {
                foreach (string primitiveTypeName in PureTypeManager.PRIMITIVE_TYPE_NAMES)
                {
                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, primitiveTypeName),
                        GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                    {
                        int index = line.LastIndexOf(' ');
                        Debug.Assert(index > 0);

                        uint size = 0;
                        bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                        Debug.Assert(bSuccess);

                        TypeInfo primitiveTypeInfo = new TypeInfo();
                        primitiveTypeInfo.SetByString(primitiveTypeName);
                        primitiveTypeInfo.Size = size;

                        PureTypeManager.AddType(primitiveTypeName, primitiveTypeInfo);
                    });
                }

                //RequestInstruction(GdbInstructionSet.UNLIMITED_NESTED_TYPE, null, null);
                RequestInstruction(GdbInstructionSet.SET_PRINT_OBJECT_ON, null, null);
                RequestInstruction(GdbInstructionSet.SET_PRINT_VIRTUAL_TABLE_ON, null, null);
                RequestInstruction(GdbInstructionSet.SET_PRINT_STATIC_MEMBERS_OFF, null, null);
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

            return true;
        }

        public void ShutdownGdb()
        {
            Log = string.Empty;
            LinePointer = 0;

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
                                if (0 == signature.Length)
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
                    heap.TypeInfoOrNull = null;

                    uint heapWordCount = heap.Size / TypeInfo.POINTER_SIZE + (heap.Size % TypeInfo.POINTER_SIZE > 0 ? 1u : 0);
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
                        uint size = 0;
                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, local.Name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                        {
                            // "$2 = 24"
                            int index = line.LastIndexOf(' ');
                            Debug.Assert(index > 0);

                            bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                            Debug.Assert(bSuccess);
                            local.StackMemory.TypeInfoOrNull.Size = size;
                            
                            uint wordCount = size / TypeInfo.POINTER_SIZE + (uint)(size % TypeInfo.POINTER_SIZE > 0 ? 1 : 0);
                            Debug.Assert(null == local.StackMemory.ByteValues);
                            local.StackMemory.ByteValues = new byte[wordCount * TypeInfo.POINTER_SIZE];
                        });

                        List<string> singleOrTwoTypeNames = new List<string>();
                        string unregisteredTypeName = null;
                        // TypeName Only
                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPENAME, local.Name),
                            GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                        {
                            singleOrTwoTypeNames.Add(line);
                        });
                        if (singleOrTwoTypeNames.Count == 1) // [0]: type
                        {
                            unregisteredTypeName = singleOrTwoTypeNames[0].Substring("type = ".Length);
                        }
                        else if (singleOrTwoTypeNames.Count == 2) // [0]: real type, [1]: type
                        {
                            Match matchDerivedRealType = RegexSet.REGEX_DERIVED_REAL_TYPE.Match(singleOrTwoTypeNames[0]);
                            Debug.Assert(matchDerivedRealType.Success);
                            unregisteredTypeName = matchDerivedRealType.Groups[1].Value;
                        }
                        else
                        {
                            Debug.Assert(singleOrTwoTypeNames.Count > 0 && singleOrTwoTypeNames.Count <= 2, "invalid count");
                        }

                        // convert raw type (ex: std::string -> std::__cxx11::basic_string<char, std::char_traits<char>, std::allocator<char> >)
                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPENAME, unregisteredTypeName),
                            GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                        {
                            unregisteredTypeName = line.Substring("type = ".Length);
                        });

                        // "type = xxx"
                        local.StackMemory.TypeInfoOrNull.SetByString(unregisteredTypeName);

                        // pure type
                        if (!PureTypeManager.HasType(local.StackMemory.TypeInfoOrNull.PureName))
                        {
                            TypeInfo newPureType = generateTypeRecursive(local.StackMemory.TypeInfoOrNull.PureName, 0, null);
                            if (!PureTypeManager.HasType(newPureType.FullNameOrNull))
                            {
                                PureTypeManager.AddType(newPureType.FullNameOrNull, newPureType);
                            }
                        }
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

                        uint wordCount = local.StackMemory.TypeInfoOrNull.Size / TypeInfo.POINTER_SIZE + (uint)(local.StackMemory.TypeInfoOrNull.Size % TypeInfo.POINTER_SIZE > 0 ? 1 : 0);

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
                            uint totalLength = local.StackMemory.TypeInfoOrNull.GetTotalLength();
                            uint sizePerSegment = local.StackMemory.TypeInfoOrNull.Size / totalLength;

                            if (0 == local.StackMemory.TypeInfoOrNull.PointerLevel && 0 == local.StackMemory.TypeInfoOrNull.ArrayOrFunctionPointerLevels.Count)
                            {
                                var pureTypeOrNull = PureTypeManager.GetType(local.StackMemory.TypeInfoOrNull.PureName);
                                if (pureTypeOrNull != null)
                                {
                                    local.StackMemory.TypeInfoOrNull.Members = PureTypeManager.GetType(local.StackMemory.TypeInfoOrNull.PureName).Members;
                                }
                            }

                            for (uint j = 0; j < totalLength; ++j)
                            {
                                stackTypes.Push(new StackKey()
                                {
                                    Type = local.StackMemory.TypeInfoOrNull.GetElementOfArray(),
                                    StartOffset = j * sizePerSegment
                                });
                            }
                        }
                        
                        while (stackTypes.Count > 0)
                        {
                            StackKey pop = stackTypes.Pop();

                            uint totalLength = pop.Type.GetTotalLength();
                            uint sizePerSegment = pop.Type.Size / totalLength;

                            if (0 == pop.Type.PointerLevel && 0 == pop.Type.ArrayOrFunctionPointerLevels.Count)
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
                                    uint offset = pop.StartOffset + pop.Type.Offset + j * TypeInfo.POINTER_SIZE;
                                    uint address = local.StackMemory.ByteValues[offset] +
                                        ((uint)local.StackMemory.ByteValues[offset + 1] << 8) +
                                        ((uint)local.StackMemory.ByteValues[offset + 2] << 16) +
                                        ((uint)local.StackMemory.ByteValues[offset + 3] << 24);

                                    HeapMemoryInfo heapOrNull = HeapManagerOrNull.GetHeapOrNull(address);
                                    if (heapOrNull != null)
                                    {
                                        heapOrNull.TypeInfoOrNull = pop.Type.GetDereference();
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
                    if (null == heap.TypeInfoOrNull)
                    {
                        continue;
                    }

                    {
                        uint sizePerSegment = heap.TypeInfoOrNull.Size;
                        uint totalLength = heap.Size / sizePerSegment;

                        for (uint i = 0; i < totalLength; ++i)
                        {
                            stackTypes.Push(new StackKey()
                            {
                                Type = heap.TypeInfoOrNull.GetElementOfArray(),
                                StartOffset = i * sizePerSegment
                            });
                        }
                    }

                    while (stackTypes.Count > 0)
                    {
                        StackKey pop = stackTypes.Pop();

                        uint totalLength = pop.Type.GetTotalLength();
                        uint sizePerSegment = pop.Type.Size / totalLength;

                        if (0 == pop.Type.PointerLevel && 0 == pop.Type.ArrayOrFunctionPointerLevels.Count)
                        {
                            for (uint i = 0; i < totalLength; ++i)
                            {
                                foreach (TypeInfo member in pop.Type.Members)
                                {
                                    stackTypes.Push(new StackKey
                                    {
                                        Type = member,
                                        StartOffset = member.Offset + i * totalLength
                                    });
                                }
                            }
                        }
                        else if (pop.Type.PointerLevel > 0)
                        {
                            // array heap
                            for (uint i = 0; i < totalLength; ++i)
                            {
                                uint offset = pop.Type.Offset + i * TypeInfo.POINTER_SIZE;
                                uint address = heap.ByteValues[offset]
                                    | ((uint)heap.ByteValues[offset + 1] << 8)
                                    | ((uint)heap.ByteValues[offset + 2] << 16)
                                    | ((uint)heap.ByteValues[offset + 3] << 24);

                                HeapMemoryInfo anotherHeapOrNull = HeapManagerOrNull.GetHeapOrNull(address);
                                if (null != anotherHeapOrNull && null == anotherHeapOrNull.TypeInfoOrNull)
                                {
                                    anotherHeapOrNull.TypeInfoOrNull = pop.Type.GetDereference();
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
            Debug.Assert(null != start);
            Debug.Assert(null != end);

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
                    if (0 == line.Length)
                    {
                        continue;
                    }
                }
#if GDBLOG
                Log += line + Environment.NewLine;
                Console.WriteLine(line);
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
                    if (0 == line.Length)
                    {
                        continue;
                    }
                }
#if GDBLOG
                Log += line + Environment.NewLine;
                Console.WriteLine(line);
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

        private TypeInfo generateTypeRecursive(string typeName, uint baseOffset, TypeInfo backupOrNull)
        {
            Debug.Assert(null != typeName);
            Debug.Assert(typeName.Length > 0);

            List<string> lines = new List<string>(128);
            RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPEINFO, typeName),
                GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
            ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
            {
                lines.Add(line);
            });
            Debug.Assert(lines.Count > 0);

            // single line (primitive types)
            if (1 == lines.Count)
            {
                // convert to raw type
                typeName = lines[0].Substring("type = ".Length);

                EMemoryTypeFlags enumFlag = EMemoryTypeFlags.NONE;
                if (typeName.StartsWith("enum "))
                {
                    enumFlag = EMemoryTypeFlags.ENUM;
                    // "enum std::_Rb_tree_color : unsigned int {std::_S_red, std::_S_black}"
                    Match match = RegexSet.REGEX_NAMED_ENUM.Match(typeName);
                    Debug.Assert(match.Success);

                    typeName = match.Groups[1].Value;
                }

                if (null != backupOrNull)
                {
                    // set type name
                    backupOrNull.SetByString(typeName);
                    backupOrNull.Flags |= enumFlag;

                    return backupOrNull;
                }
                else
                {
                    var typeInfo = new TypeInfo();

                    typeInfo.SetByString(typeName);
                    typeInfo.Flags |= enumFlag;

                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, typeName),
                        GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                    {
                        int index = line.LastIndexOf(' ');
                        Debug.Assert(index > 0);

                        uint size = 0;
                        bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                        Debug.Assert(bSuccess);
                        typeInfo.Size = size;
                    });

                    return typeInfo;
                }
            }
            // multiple lines (struct, class, union, etc.)
            else
            {
                Debug.Assert(lines.Count > 0);
                {
                    uint size = 0;

                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_SIZEOF, typeName),
                        GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_SIZEOF, GdbInstructionSet.REQUEST_END_DISPLAY_SIZEOF, (string line) =>
                    {
                        int index = line.LastIndexOf(' ');
                        Debug.Assert(index > 0);

                        bool bSuccess = uint.TryParse(line.Substring(index + 1), out size);
                        Debug.Assert(bSuccess);
                    });

                    string offsetAndSizeHeader = "/* offset    |  size */";

                    lines[0] = lines[0].Replace("type = ", string.Empty);

                    if (lines[0].StartsWith(offsetAndSizeHeader))
                    {
                        lines[0] = string.Format("/*    0      |     {0} */", size) + lines[0].Substring(offsetAndSizeHeader.Length);
                    }
                    else
                    {
                        lines[0] = string.Format("/*    0      |     {0} */", size) + lines[0];
                    }
                }

                Stack<TypeInfo> stack = new Stack<TypeInfo>();

                foreach (string line in lines)
                {
                    Match matchOffsetAndSize = RegexSet.REGEX_OFFSET_AND_SIZE.Match(line);
                    if (matchOffsetAndSize.Success)
                    {
                        TypeInfo typeInfo = new TypeInfo();

                        uint offset = 0;
                        uint size = 0;
                        bool bSuccess = false;

                        if (matchOffsetAndSize.Groups[2].Value.Length > 0)
                        {
                            // "/*    0      |    24 */    std::string name;"
                            bSuccess = uint.TryParse(matchOffsetAndSize.Groups[2].Value, out offset);
                            Debug.Assert(bSuccess);
                            typeInfo.Offset = baseOffset + offset;
                        }
                        else // all members in union have same offset
                        {
                            typeInfo.Offset = stack.Peek().Offset;
                        }

                        bSuccess = uint.TryParse(matchOffsetAndSize.Groups[3].Value, out size);
                        Debug.Assert(bSuccess);
                        typeInfo.Size = size;

                        Match matchCseu;
                        
                        if ((matchCseu = RegexSet.REGEX_CLASS_OR_STRUCT.Match(line)).Success)
                        {
                            switch (matchCseu.Groups[1].Value)
                            {
                                case "class":
                                    typeInfo.Flags |= EMemoryTypeFlags.CLASS;
                                    break;

                                case "struct":
                                    typeInfo.Flags |= EMemoryTypeFlags.STRUCT;
                                    break;

                                default:
                                    Debug.Assert(false, "invalid type");
                                    break;
                            }

                            int index = matchCseu.Groups[2].Value.IndexOf(" : ");
                            // if inheritance exists
                            if (index >= 0)
                            {
                                string inheritanceFormat = matchCseu.Groups[2].Value.Substring(index + 3);
                                string[] inheritanceTypeNames = RegexSet.REGEX_INHERITANCE.Replace(inheritanceFormat, "|").Split('|');

                                for (int i = 1; i < inheritanceTypeNames.Length; ++i)
                                {
                                    TypeInfo parentType = generateTypeRecursive(inheritanceTypeNames[i], typeInfo.Offset, null);
                                    foreach (var member in parentType.Members)
                                    {
                                        typeInfo.Members.Add(member);
                                    }
                                }

                                typeInfo.SetByString(matchCseu.Groups[2].Value.Substring(0, index));
                            }
                            else if (matchCseu.Groups[2].Value.Length > 0)
                            {
                                typeInfo.SetByString(matchCseu.Groups[2].Value);
                            }

                            stack.Push(typeInfo);
                        }
                        else if ((matchCseu = RegexSet.REGEX_ENUM.Match(line)).Success)
                        {
                            typeInfo.Flags |= EMemoryTypeFlags.ENUM;
                            typeInfo.MemberNameOrNull = matchCseu.Groups[1].Value;

                            stack.Peek().Members.Add(typeInfo);
                        }
                        else if ((matchCseu = RegexSet.REGEX_UNION.Match(line)).Success)
                        {
                            typeInfo.Flags |= EMemoryTypeFlags.UNION;
                            stack.Push(typeInfo);
                        }
                        // "/*   48      |     8 */    double age;"
                        else if (';' == line[line.Length - 1])
                        {
                            Match matchMemberName = RegexSet.REGEX_MEMBER_NAME.Match(line);
                            Debug.Assert(matchMemberName.Success);

                            typeInfo.MemberNameOrNull = matchMemberName.Value;

                            int lastIndex = line.LastIndexOf(matchMemberName.Value);
                            Debug.Assert(-1 != lastIndex);

                            string fullTypeName = line.Remove(lastIndex, matchMemberName.Value.Length);
                            fullTypeName = fullTypeName.Substring(23);
                            fullTypeName = fullTypeName.Substring(0, fullTypeName.Length - 1).Trim();

                            var childTypeInfo = generateTypeRecursive(fullTypeName, stack.Peek().Offset + offset, typeInfo);

                            stack.Peek().Members.Add(childTypeInfo);

                            if (!PureTypeManager.HasType(childTypeInfo.PureName))
                            {
                                PureTypeManager.AddType(childTypeInfo.PureName, generateTypeRecursive(childTypeInfo.PureName, 0, null));
                            }
                        }
                    }
                    // "                           } v;"
                    // "                           };"
                    else if (';' == line[line.Length - 1] && line.Contains("static") == false)
                    {
                        int closingBracketIndex = line.IndexOf('}');
                        Debug.Assert(closingBracketIndex >= 0);

                        string chunk = line.Substring(closingBracketIndex + 1);
                        chunk = chunk.Substring(0, chunk.Length - 1);

                        Match matchMemberName = RegexSet.REGEX_MEMBER_NAME.Match(chunk);
                        if (matchMemberName.Success)
                        {
                            int index = chunk.IndexOf(matchMemberName.Value);
                            Debug.Assert(index >= 0);

                            stack.Peek().MemberNameOrNull = matchMemberName.Value;

                            string mergedTypeName = (stack.Peek().FullNameOrNull + ' ' + chunk.Remove(index, matchMemberName.Value.Length)).Trim();
                            if (mergedTypeName.Length > 0)
                            {
                                stack.Peek().SetByString(mergedTypeName);
                            }
                        }

                        var pop = stack.Pop();
                        stack.Peek().Members.Add(pop);
                    }
                    // "                         } [5]"
                    else
                    {
                        int closingBracketIndex = line.IndexOf('}');
                        if (closingBracketIndex >= 0)
                        {
                            Debug.Assert(stack.Count == 1);
                            Debug.Assert(line.Contains(';') == false);

                            string mergedTypeName = (stack.Peek().FullNameOrNull + ' ' + line.Substring(closingBracketIndex + 1)).Trim();

                            stack.Peek().SetByString(mergedTypeName);
                        }
                    }
                }

                Debug.Assert(stack.Count == 1);

                return stack.Peek();
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
