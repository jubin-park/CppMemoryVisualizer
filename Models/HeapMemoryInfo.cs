using CppMemoryVisualizer.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace CppMemoryVisualizer.Models
{
    sealed class HeapMemoryInfo : MemoryOwnerInfo, INotifyPropertyChanged
    {
        private uint mSize;
        public uint Size
        {
            get
            {
                return mSize;
            }
        }

        private bool mbVisible = true;
        public bool IsVisible
        {
            get
            {
                return mbVisible;
            }
            set
            {
                mbVisible = value;
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

        public HeapMemoryInfo(uint address, uint size)
        {
            mAddress = address;
            mSize = size;

            uint wordCount = size / 4 + (size % 4 > 0 ? 1u : 0);
            mByteValues = new byte[wordCount * 4];
        }

        public void UpdateMemorySegements()
        {
            if (TypeInfo == null || TypeInfo.PureName == null)
            {
                return;
            }

            List<MemorySegmentViewModel> segments;
            var stackKeys = new Stack<StackKey>();

            {
                TypeInfo pureType = PureTypeManager.GetType(TypeInfo.PureName);
                uint sizePerSegment = TypeInfo.PointerLevel > 0 ? 4 : pureType.Size;
                uint totalLength = Size / sizePerSegment;
                uint remainSegment = Size % sizePerSegment;

                segments = new List<MemorySegmentViewModel>((int)totalLength);

                for (uint i = 0; i < totalLength; ++i)
                {
                    var vm = new MemorySegmentViewModel()
                    {
                        TypeName = TypeInfo.FullNameOrNull,
                        MemberNameOrNull = TypeInfo.MemberNameOrNull,
                        Memory = new ArraySegment<byte>(ByteValues, (int)(i * sizePerSegment), (int)sizePerSegment),
                        Address = Address + i * sizePerSegment,
                        AncestorOrNull = null,
                        Children = new List<List<MemorySegmentViewModel>>(pureType.Members.Count)
                    };

                    segments.Add(vm);
                    stackKeys.Push(new StackKey()
                    {
                        ViewModel = vm,
                        Type = TypeInfo
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
                            Memory = new ArraySegment<byte>(ByteValues, popKey.ViewModel.Memory.Offset + (int)((memberType.Offset - popKey.Type.Offset) + i * sizePerSegment), (int)sizePerSegment),
                            Address = popKey.ViewModel.Address + (memberType.Offset - popKey.Type.Offset) + i * sizePerSegment,
                            AncestorOrNull = popKey.ViewModel,
                            Children = new List<List<MemorySegmentViewModel>>(memberType.Members.Count)
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
