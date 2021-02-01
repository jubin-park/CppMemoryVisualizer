using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private double mX;
        public double X
        {
            get
            {
                return mX;
            }
            set
            {
                mX = value;
                OnPropertyChanged("X");
            }
        }

        private double mY;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Y
        {
            get
            {
                return mY;
            }
            set
            {
                mY = value;
                OnPropertyChanged("Y");
            }
        }

        public HeapMemoryInfo(uint address, uint size)
        {
            mAddress = address;
            mSize = size;

            uint wordCount = size / 4 + (size % 4 > 0 ? 1u : 0);
            mByteValues = new byte[wordCount * 4];
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
