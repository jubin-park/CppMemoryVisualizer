using CppMemoryVisualizer.Models;
using CppMemoryVisualizer.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CppMemoryVisualizer.Converters
{
    sealed class StackMemoryToSegmentsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<StackMemorySegmentViewModel> segments;
            var stack = new Stack<StackKey>();

            MemoryOwnerInfo rootMemory = value as MemoryOwnerInfo;
            {
                TypeInfo pureType = PureTypeManager.GetType(rootMemory.TypeInfo.PureName);

                uint totalLength = 1;
                if (rootMemory.TypeInfo.ArrayOrFunctionPointerLevels.Count == 0)
                {
                    foreach (uint len in rootMemory.TypeInfo.ArrayLengths)
                    {
                        totalLength *= len;
                    }
                }
                uint sizePerSegment = rootMemory.TypeInfo.Size / totalLength;

                TypeInfo elementOfArrayType = rootMemory.TypeInfo.GetElementOfArray();

                segments = new List<StackMemorySegmentViewModel>((int)totalLength);

                if (rootMemory.TypeInfo.PointerLevel == 0 && rootMemory.TypeInfo.ArrayOrFunctionPointerLevels.Count == 0)
                {
                    rootMemory.TypeInfo.Members = pureType.Members;
                }

                for (uint i = 0; i < totalLength; ++i)
                {
                    var vm = new StackMemorySegmentViewModel()
                    {
                        TypeName = totalLength > 1 ? elementOfArrayType.FullNameOrNull : rootMemory.TypeInfo.FullNameOrNull,
                        MemberNameOrNull = rootMemory.TypeInfo.MemberNameOrNull,
                        Memory = new ArraySegment<byte>(rootMemory.ByteValues, (int)(i * sizePerSegment), (int)sizePerSegment),
                        Address = rootMemory.Address + i * sizePerSegment,
                        AncestorOrNull = null,
                        Children = new List<List<StackMemorySegmentViewModel>>(pureType.Members.Count)
                    };

                    segments.Add(vm);
                    stack.Push(new StackKey()
                    {
                        ViewModel = vm,
                        Type = rootMemory.TypeInfo
                    });
                }
            }

            while (stack.Count > 0)
            {
                StackKey popKey = stack.Pop();
                
                if (popKey.Type.PointerLevel == 0 && popKey.Type.ArrayOrFunctionPointerLevels.Count == 0)
                {
                    foreach (TypeInfo memberType in popKey.Type.Members)
                    {
                        uint totalLength = 1;
                        if (memberType.ArrayOrFunctionPointerLevels.Count == 0)
                        {
                            foreach (uint len in memberType.ArrayLengths)
                            {
                                totalLength *= len;
                            }
                        }
                        uint sizePerSegment = memberType.Size / totalLength;

                        TypeInfo elementOfArrayType = memberType.GetElementOfArray();

                        var memberArray = new List<StackMemorySegmentViewModel>((int)totalLength);
                        for (uint i = 0; i < totalLength; ++i)
                        {
                            var vm = new StackMemorySegmentViewModel()
                            {
                                TypeName = totalLength > 1 ? elementOfArrayType.FullNameOrNull : memberType.FullNameOrNull,
                                MemberNameOrNull = memberType.MemberNameOrNull,
                                Memory = new ArraySegment<byte>(rootMemory.ByteValues, popKey.ViewModel.Memory.Offset + (int)((memberType.Offset - popKey.Type.Offset) + i * sizePerSegment), (int)sizePerSegment),
                                Address = popKey.ViewModel.Address + (memberType.Offset - popKey.Type.Offset) + i * sizePerSegment,
                                AncestorOrNull = popKey.ViewModel,
                                Children = new List<List<StackMemorySegmentViewModel>>(memberType.Members.Count)
                            };

                            stack.Push(new StackKey()
                            {
                                ViewModel = vm,
                                Type = memberType
                            });
                            memberArray.Add(vm);
                        }

                        popKey.ViewModel.Children.Add(memberArray);
                    }
                }
            }

            return segments;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        sealed class StackKey
        {
            public StackMemorySegmentViewModel ViewModel;
            public TypeInfo Type;
        }
    }
}