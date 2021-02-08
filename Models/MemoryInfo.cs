namespace CppMemoryVisualizer.Models
{
    class MemoryInfo
    {
        protected uint mAddress;
        public uint Address
        {
            get
            {
                return mAddress;
            }
            set
            {
                mAddress = value;
            }
        }

        protected TypeInfo mTypeInfo = new TypeInfo();
        public TypeInfo TypeInfo
        {
            get
            {
                return mTypeInfo;
            }
            set
            {
                mTypeInfo = value;
            }
        }

        protected bool mbChanged;
        public bool IsChanged
        {
            get
            {
                return mbChanged;
            }
        }
    }
}
