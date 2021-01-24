﻿using CppMemoryVisualizer.Commands;
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
                RequestInstruction("set pagination off",
                    null, null);
                RequestInstruction(GdbInstructionSet.SKIP_STL_CONSTRUCTOR_DESTRUCTOR,
                    null, null);
                RequestInstruction(GdbInstructionSet.SET_BREAK_POINT_MAIN,
                    null, null);
                RequestInstruction(GdbInstructionSet.RUN,
                    GdbInstructionSet.REQUEST_START_INIT, GdbInstructionSet.REQUEST_END_INIT);
                ReadResultLine(GdbInstructionSet.REQUEST_START_INIT, GdbInstructionSet.REQUEST_END_INIT, ActionLinePointer);
                RequestInstruction(GdbInstructionSet.CLEAR_ALL_BREAK_POINTS,
                    null, null);
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
            {
                mCallStackViewModel.CallStack.Clear();

                Regex rx = new Regex(@"(\w+)\s\((.*)\)\sat");

                RequestInstruction(GdbInstructionSet.DISPLAY_STACK_BACKTRACE,
                    GdbInstructionSet.REQUEST_START_GET_CALL_STACK, GdbInstructionSet.REQUEST_END_GET_CALL_STACK);
                ReadResultLine(GdbInstructionSet.REQUEST_START_GET_CALL_STACK, GdbInstructionSet.REQUEST_END_GET_CALL_STACK, (string line) =>
                {
                    Match match = rx.Match(line);

                    if (match.Success)
                    {
                        string functionName = match.Groups[1].Value;
                        string[] parameters = match.Groups[2].Value.Split(',');
                    }
                });
            }
        }

        public void Update()
        {
            #region Get StackTrace
            {
                mCallStackViewModel.CallStack.Clear();

                string fileNameOnly = Path.GetFileNameWithoutExtension(mSourcePathOrNull);
                Regex rx = new Regex(@"^\d+\s([0-9a-f]{8})\s([0-9a-f]{8})\s" + fileNameOnly + @"!(.*)\s\[(.*)\s@\s(\d+)\]");

                RequestInstruction(GdbInstructionSet.DISPLAY_STACK_BACKTRACE,
                    GdbInstructionSet.REQUEST_START_GET_CALL_STACK, GdbInstructionSet.REQUEST_END_GET_CALL_STACK);
                ReadResultLine(GdbInstructionSet.REQUEST_START_GET_CALL_STACK, GdbInstructionSet.REQUEST_END_GET_CALL_STACK, (string line) => 
                {
                    Match match = rx.Match(line);

                    if (match.Success)
                    {
                        string stackAddressHex = match.Groups[1].Value;
                        string functionAddressHex = match.Groups[2].Value;
                        string name = match.Groups[3].Value;
                        string path = match.Groups[4].Value;
                        //string line = match.Groups[5].Value;

                        if (path == mSourcePathOrNull)
                        {
                            uint stackAddress = 0;
                            Debug.Assert(uint.TryParse(stackAddressHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out stackAddress));

                            uint functionAddress = 0;
                            Debug.Assert(uint.TryParse(functionAddressHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out functionAddress));

                            Debug.WriteLine("Stack addr: {0}, Function addr: {1}, name: {2}", stackAddress, functionAddress, name);
                            mCallStackViewModel.CallStack.Push(stackAddress, functionAddress, name);
                        }
                    }
                });
            }
            #endregion

            if (mCallStackViewModel.CallStack.IsEmpty())
            {
                LinePointer = 0;
                ShutdownGdb();

                return;
            }

            Models.StackFrame stackFrame = mCallStackViewModel.CallStack.GetStackFrame(mCallStackViewModel.CallStack.Top());

            #region Get Local Variable Info
            {
                bool isFailed = false;

                RequestInstruction(GdbInstructionSet.DISPLAY_LOCAL_VARIABLE,
                    GdbInstructionSet.REQUEST_START_GET_LOCAL_VARS, GdbInstructionSet.REQUEST_END_GET_LOCAL_VARS);
                ReadResultLine(GdbInstructionSet.REQUEST_START_GET_LOCAL_VARS, GdbInstructionSet.REQUEST_END_GET_LOCAL_VARS, (string line) =>
                {
                    if (!stackFrame.IsInitialized)
                    {
                        Regex rx = new Regex(@"^prv\s(local|param)\s+([0-9a-f]{8})\s+(\w+|\w+\s[a-zA-Z0-9_<>,: ]+|<function>)\s(\**)([\(\*+\)]*)([\[\d+\]]*)\s*(\w+)\s=\s");
                        Match match = rx.Match(line);

                        if (match.Success)
                        {
                            string localOrParam = match.Groups[1].Value;
                            string stackAddr = match.Groups[2].Value;
                            string pureTypeName = match.Groups[3].Value;
                            string pointerChars = match.Groups[4].Value;
                            string arrayOrFunctionPointerChars = match.Groups[5].Value;
                            string dimensions = match.Groups[6].Value;
                            string variableName = match.Groups[7].Value;
                            
                            stackFrame.TryAdd(variableName);
                            LocalVariable local = stackFrame.GetLocalVariableOrNull(variableName);

                            // local or parameter (fixed)
                            local.IsParameter = (localOrParam == "param");

                            // name (fixed)
                            local.Name = variableName;

                            // address in stack
                            uint address = 0;
                            Debug.Assert(uint.TryParse(stackAddr, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out address));
                            local.StackMemory.Address = address;

                            // type (fixed)
                            TypeInfo newType = new TypeInfo();

                            // typename
                            if (pureTypeName.StartsWith("struct "))
                            {
                                pureTypeName = pureTypeName.Substring(7);
                                newType.Flags |= EMemoryTypeFlags.STRUCT;
                            }
                            else if (pureTypeName.StartsWith("class "))
                            {
                                pureTypeName = pureTypeName.Substring(6);
                                newType.Flags |= EMemoryTypeFlags.CLASS;
                            }
                            else if (pureTypeName.StartsWith("enum "))
                            {
                                pureTypeName = pureTypeName.Substring(5);
                                newType.Flags |= EMemoryTypeFlags.ENUM;
                            }
                            else if (pureTypeName.StartsWith("union "))
                            {
                                pureTypeName = pureTypeName.Substring(6);
                                newType.Flags |= EMemoryTypeFlags.UNION;
                            }
                            else if (pureTypeName == "<function>")
                            {
                                newType.Flags |= EMemoryTypeFlags.FUNCTION;
                            }

                            // STL
                            if (pureTypeName.Contains("std::"))
                            {
                                newType.Flags |= EMemoryTypeFlags.STL;
                            }

                            newType.PureName = pureTypeName;

                            // Pointer
                            if (pointerChars.Length > 0 || arrayOrFunctionPointerChars.Length > 0)
                            {
                                newType.Flags |= EMemoryTypeFlags.POINTER;
                            }

                            // Array
                            if (dimensions.Length > 0)
                            {
                                newType.Flags |= EMemoryTypeFlags.ARRAY;
                            }
                            newType.PointerLevel = (uint)pointerChars.Length;

                            // ArrayOrFunctionPointer
                            {
                                Regex regex = new Regex(@"\((\*+)\)");
                                Match matchPointer = regex.Match(arrayOrFunctionPointerChars);

                                while (matchPointer.Success)
                                {
                                    uint size = (uint)matchPointer.Groups[1].Length;
                                    newType.ArrayOrFunctionPointerLevels.Add(size);

                                    matchPointer = matchPointer.NextMatch();
                                }
                            }

                            // Array Lengths
                            {
                                Regex regex = new Regex(@"\[(\d+)\]");
                                Match matchDimension = regex.Match(dimensions);

                                while (matchDimension.Success)
                                {
                                    uint size = 0;
                                    Debug.Assert(uint.TryParse(matchDimension.Groups[1].Value, out size));

                                    newType.ArrayLengths.Add(size);

                                    matchDimension = matchDimension.NextMatch();
                                }
                            }

                            if (local.StackMemory.TypeInfo == null)
                            {
                                local.StackMemory.TypeInfo = newType;
                            }
                        }
                    }
                    // Update stack address only
                    else
                    {
                        Regex rx = new Regex(@"\s{2}([0-9a-f]{8}).*\s(\w+)\s=\s");
                        Match match = rx.Match(line);

                        if (match.Success)
                        {
                            string stackAddr = match.Groups[1].Value;
                            string variableName = match.Groups[2].Value;
                            LocalVariable local = stackFrame.GetLocalVariableOrNull(variableName);

                            if (local == null)
                            {
                                isFailed = true;
                                stackFrame.IsInitialized = false;
                            }
                            else
                            {
                                uint address = 0;
                                Debug.Assert(uint.TryParse(stackAddr, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out address));
                                local.StackMemory.Address = address;
                            }
                        }
                    }
                });

                if (isFailed)
                {
                    return;
                }
            }
            #endregion

            // Check if initialized
            if (!mCallStackViewModel.CallStack.IsEmpty() && stackFrame.IsInitialized)
            {
                goto UpdateMemory;
            }

            #region Get Local Variable SizeOf
            {
                foreach (var name in stackFrame.LocalVariableNames)
                {
                    LocalVariable local = stackFrame.GetLocalVariableOrNull(name);

                    RequestInstruction(string.Format(GdbInstructionSet.EVALUATE_SIZEOF, name),
                        GdbInstructionSet.REQUEST_START_SIZEOF + ' ' + name, GdbInstructionSet.REQUEST_END_SIZEOF);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_SIZEOF, GdbInstructionSet.REQUEST_END_SIZEOF, (string line) =>
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
                        local.StackMemory.TypeInfo.Size = size;

                        uint wordSize = size / 4 + (uint)(size % 4 > 0 ? 1 : 0);

                        if (local.StackMemory.ByteValues == null)
                        {
                            local.StackMemory.ByteValues = new byte[wordSize * 4];
                        }
                    });
                }                
            }
            #endregion

            #region Get Struct/Class Members
            {
                Queue<string> memberQueue = new Queue<string>();

                foreach (var name in stackFrame.LocalVariableNames)
                {
                    LocalVariable local = stackFrame.GetLocalVariableOrNull(name);
                    TypeInfo typeInfo = local.StackMemory.TypeInfo;
                    string pureName = typeInfo.PureName;

                    if (mPureTypeManagerOrNull.HasType(pureName))
                    {
                        local.StackMemory.PureTypeInfo = mPureTypeManagerOrNull.GetType(pureName);
                        continue;
                    }

                    TypeInfo pure = new TypeInfo();
                    pure.PureName = pureName;
                    
                    RequestInstruction(string.Format(GdbInstructionSet.EVALUATE_SIZEOF, pureName),
                        GdbInstructionSet.REQUEST_START_SIZEOF + ' ' + pureName, GdbInstructionSet.REQUEST_END_SIZEOF);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_SIZEOF, GdbInstructionSet.REQUEST_END_SIZEOF, (string innerLine) =>
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

                        pure.Size = size;
                    });

                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPE, pureName),
                        GdbInstructionSet.REQUEST_START_DISPLAY_TYPE + ' ' + pureName, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                    {
                        Regex rx = new Regex(@"^\+0x([0-9a-f]+)\s(\w+)\s+:\s(Ptr32|[\[\d+\]]+\s\w+$|\w+$)");
                        Match match = rx.Match(line);

                        if (match.Success)
                        {
                            TypeInfo newMember = new TypeInfo();

                            string offsetHex = match.Groups[1].Value;
                            string memberName = match.Groups[2].Value;
                            string arrayTypeOrType = match.Groups[3].Value;

                            // offset
                            uint offset = 0;
                            Debug.Assert(uint.TryParse(offsetHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out offset));
                            newMember.Offset = offset;

                            // member name
                            newMember.MemberNameOrNull = memberName;

                            // arrayTypeOrType name
                            int lastIndex = arrayTypeOrType.LastIndexOf(' ');
                            if (lastIndex >= 0) // array
                            {
                                string dimensions = arrayTypeOrType.Substring(0, lastIndex);
                                string memberTypeName = arrayTypeOrType.Substring(lastIndex + 1);

                                Regex regex = new Regex(@"\[(\d+)\]");
                                Match matchDimension = regex.Match(dimensions);

                                newMember.PureName = memberTypeName;
                                memberQueue.Enqueue(memberTypeName);

                                while (matchDimension.Success)
                                {
                                    uint size = 0;
                                    Debug.Assert(uint.TryParse(matchDimension.Groups[1].Value, out size));

                                    newMember.ArrayLengths.Add(size);

                                    matchDimension = matchDimension.NextMatch();
                                }
                            }
                            else
                            {
                                newMember.PureName = arrayTypeOrType;
                                memberQueue.Enqueue(arrayTypeOrType);
                            }

                            pure.Members.Add(newMember);
                        }
                    });

                    local.StackMemory.PureTypeInfo = pure;
                    mPureTypeManagerOrNull.AddType(pureName, pure);
                }

                #region member Queue
                {
                    while (memberQueue.Count > 0)
                    {
                        string pureName = memberQueue.Dequeue();
                        if (mPureTypeManagerOrNull.HasType(pureName))
                        {
                            continue;
                        }

                        TypeInfo pure = new TypeInfo();
                        pure.PureName = pureName;

                        RequestInstruction(string.Format(GdbInstructionSet.EVALUATE_SIZEOF, pureName),
                            GdbInstructionSet.REQUEST_START_SIZEOF + ' ' + pureName, GdbInstructionSet.REQUEST_END_SIZEOF);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_SIZEOF, GdbInstructionSet.REQUEST_END_SIZEOF, (string innerLine) =>
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

                            pure.Size = size;
                        });

                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_TYPE, pureName),
                            GdbInstructionSet.REQUEST_START_DISPLAY_TYPE + ' ' + pureName, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_TYPE, GdbInstructionSet.REQUEST_END_DISPLAY_TYPE, (string line) =>
                        {
                            Regex rx = new Regex(@"^\+0x([0-9a-f]+)\s(\w+)\s+:\s(Ptr32|[\[\d+\]]+\s\w+$|\w+$)");
                            Match match = rx.Match(line);

                            if (match.Success)
                            {
                                TypeInfo newMember = new TypeInfo();

                                string offsetHex = match.Groups[1].Value;
                                string memberName = match.Groups[2].Value;
                                string arrayTypeOrType = match.Groups[3].Value;

                                // offset
                                uint offset = 0;
                                Debug.Assert(uint.TryParse(offsetHex, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out offset));
                                newMember.Offset = offset;

                                // member name
                                newMember.MemberNameOrNull = memberName;

                                // arrayTypeOrType name
                                int lastIndex = arrayTypeOrType.LastIndexOf(' ');
                                if (lastIndex >= 0) // array
                                {
                                    string dimensions = arrayTypeOrType.Substring(0, lastIndex);
                                    string memberTypeName = arrayTypeOrType.Substring(lastIndex + 1);

                                    Regex regex = new Regex(@"\[(\d+)\]");
                                    Match matchDimension = regex.Match(dimensions);

                                    newMember.PureName = memberTypeName;
                                    memberQueue.Enqueue(memberTypeName);

                                    while (matchDimension.Success)
                                    {
                                        uint size = 0;
                                        Debug.Assert(uint.TryParse(matchDimension.Groups[1].Value, out size));

                                        newMember.ArrayLengths.Add(size);

                                        matchDimension = matchDimension.NextMatch();
                                    }
                                }
                                else
                                {
                                    newMember.PureName = arrayTypeOrType;
                                    memberQueue.Enqueue(arrayTypeOrType);
                                }

                                pure.Members.Add(newMember);
                            }
                        });

                        mPureTypeManagerOrNull.AddType(pureName, pure);
                    }
                    #endregion
                }

                stackFrame.IsInitialized = true;
            }
            #endregion

            UpdateMemory:

            #region Get Memory Words
            {
                foreach (var name in stackFrame.LocalVariableNames)
                {
                    LocalVariable local = stackFrame.GetLocalVariableOrNull(name);
                    RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_MEMORY, local.StackMemory.ByteValues.Length / 4, "0x" + local.StackMemory.Address.ToString("X")),
                        GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY + ' ' + name, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY);
                    ReadResultLine(GdbInstructionSet.REQUEST_START_DISPLAY_MEMORY, GdbInstructionSet.REQUEST_END_DISPLAY_MEMORY, (string line) =>
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
                    LocalVariable local = stackFrame.GetLocalVariableOrNull(name);
                    if (local.StackMemory.TypeInfo.Flags.HasFlag(EMemoryTypeFlags.POINTER) && local.StackMemory.IsChanged)
                    {
                        byte[] byteValues = local.StackMemory.ByteValues;
                        uint wordValue = ((uint)byteValues[0] << 24) | ((uint)byteValues[1] << 16) | ((uint)byteValues[2] << 8) | (uint)byteValues[3];

                        RequestInstruction(string.Format(GdbInstructionSet.DISPLAY_HEAP, wordValue.ToString("X")),
                            GdbInstructionSet.REQUEST_START_HEAP + ' ' + name, GdbInstructionSet.REQUEST_END_HEAP);
                        ReadResultLine(GdbInstructionSet.REQUEST_START_HEAP, GdbInstructionSet.REQUEST_END_HEAP, (string line) =>
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
                                if (local.StackMemory.TypeInfo.PointerLevel > 0)
                                {
                                    Debug.Assert(mPureTypeManagerOrNull.HasType(local.StackMemory.TypeInfo.PureName));

                                    uint unitSize = local.StackMemory.TypeInfo.PointerLevel == 1 ? mPureTypeManagerOrNull.GetType(local.StackMemory.TypeInfo.PureName).Size : 4;
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
            Regex rx = new Regex(@"^(\d+)\t(.*)");
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
