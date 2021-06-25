using CppMemoryVisualizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace CppMemoryVisualizer.Models
{
    sealed class LocalVariable : INotifyPropertyChanged
    {
        private bool mbArgument;
        public bool IsArgument
        {
            get
            {
                return mbArgument;
            }
        }

        private string mName;
        public string Name
        {
            get
            {
                return mName;
            }
        }

        private readonly MemoryOwnerInfo mStackMemory = new MemoryOwnerInfo();
        public MemoryOwnerInfo StackMemory
        {
            get
            {
                return mStackMemory;
            }
        }

        private List<MemorySegmentViewModel> mMemorySegments;
        public List<MemorySegmentViewModel> MemorySegments
        {
            get
            {
                return mMemorySegments;
            }
            set
            {
                mMemorySegments = value;
                onPropertyChanged("MemorySegments");
            }
        }

        public LocalVariable(string name, bool isArgument)
        {
            Debug.Assert(name != null);

            mName = name;
            mbArgument = isArgument;
        }

        public void UpdateMemorySegments()
        {
            List<MemorySegmentViewModel> segments;
            var stackKeys = new Stack<StackKey>();

            {
                uint totalLength = mStackMemory.TypeInfoOrNull.GetTotalLength();
                uint sizePerSegment = mStackMemory.TypeInfoOrNull.Size / totalLength;

                TypeInfo elementOfArrayType = mStackMemory.TypeInfoOrNull.GetElementOfArray();

                segments = new List<MemorySegmentViewModel>((int)totalLength);

                for (uint i = 0; i < totalLength; ++i)
                {
                    var vm = new MemorySegmentViewModel()
                    {
                        TypeName = totalLength > 1 ? elementOfArrayType.FullNameOrNull : mStackMemory.TypeInfoOrNull.FullNameOrNull,
                        MemberNameOrNull = mStackMemory.TypeInfoOrNull.MemberNameOrNull,
                        Memory = new ArraySegment<byte>(mStackMemory.ByteValues, (int)(i * sizePerSegment), (int)sizePerSegment),
                        Address = mStackMemory.Address + i * sizePerSegment,
                        AncestorOrNull = null,
                        Children = new ObservableCollection<List<MemorySegmentViewModel>>()
                    };

                    segments.Add(vm);
                    stackKeys.Push(new StackKey()
                    {
                        ViewModel = vm,
                        Type = mStackMemory.TypeInfoOrNull
                    });
                }
            }

            while (stackKeys.Count > 0)
            {
                StackKey popKey = stackKeys.Pop();

                if (popKey.Type.PointerLevel > 0 || popKey.Type.ArrayOrFunctionPointerLevels.Count > 0)
                {
                    continue;
                }

                foreach (TypeInfo memberType in popKey.Type.Members)
                {
                    uint totalLength = memberType.GetTotalLength();
                    uint sizePerSegment = memberType.Size / totalLength;

                    TypeInfo elementOfArrayType = memberType.GetElementOfArray();

                    var memberArray = new List<MemorySegmentViewModel>((int)totalLength);
                    for (uint i = 0; i < totalLength; ++i)
                    {
                        var vm = new MemorySegmentViewModel()
                        {
                            TypeName = totalLength > 1 ? elementOfArrayType.FullNameOrNull : memberType.FullNameOrNull,
                            MemberNameOrNull = memberType.MemberNameOrNull,
                            Memory = new ArraySegment<byte>(mStackMemory.ByteValues, popKey.ViewModel.Memory.Offset + (int)((memberType.Offset - popKey.Type.Offset) + i * sizePerSegment), (int)sizePerSegment),
                            Address = popKey.ViewModel.Address + (memberType.Offset - popKey.Type.Offset) + i * sizePerSegment,
                            AncestorOrNull = popKey.ViewModel,
                            Children = new ObservableCollection<List<MemorySegmentViewModel>>()
                        };

                        stackKeys.Push(new StackKey()
                        {
                            ViewModel = vm,
                            Type = memberType
                        });
                        memberArray.Add(vm);
                    }
                    popKey.ViewModel.Children.Add(memberArray);
                }
            }

            MemorySegments = segments;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        sealed class StackKey
        {
            public MemorySegmentViewModel ViewModel;
            public TypeInfo Type;
        }
    }
}
