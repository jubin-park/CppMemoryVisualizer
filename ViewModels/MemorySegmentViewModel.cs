using CppMemoryVisualizer.Commands;
using CppMemoryVisualizer.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CppMemoryVisualizer.ViewModels
{
    class MemorySegmentViewModel : INotifyPropertyChanged
    {
        public static MemorySegmentAddressClickCommand AddressClickCommand;
        public static MemorySegmentPointerValueClickCommand PointerValueClickCommand;

        public string TypeName { get; set; }
        public string MemberNameOrNull { get; set; }
        public ArraySegment<byte> Memory { get; set; }
        public uint Address { get; set; }
        public MemorySegmentViewModel AncestorOrNull { get; set; }
        public ObservableCollection<List<MemorySegmentViewModel>> Children { get; set; }
        public MemorySegmentAddressClickCommand MemorySegmentAddressClickCommand { get => AddressClickCommand; }
        public MemorySegmentPointerValueClickCommand MemorySegmentPointerValueClickCommand { get => PointerValueClickCommand; }

        private EMemoryArea mCapturedAddress = EMemoryArea.UNKNOWN;
        public EMemoryArea CapturedAddress
        {
            get
            {
                return mCapturedAddress;
            }
            set
            {
                mCapturedAddress = value;
                onPropertyChanged("CapturedAddress");
            }
        }

        private EMemoryArea mCapturedValue = EMemoryArea.UNKNOWN;
        public EMemoryArea CapturedValue
        {
            get
            {
                return mCapturedValue;
            }
            set
            {
                mCapturedValue = value;
                onPropertyChanged("CapturedValue");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void onPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
