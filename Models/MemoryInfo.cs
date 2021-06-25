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

        protected TypeInfo mTypeInfoOrNull = new TypeInfo();
        public TypeInfo TypeInfoOrNull
        {
            get
            {
                return mTypeInfoOrNull;
            }
            set
            {
                mTypeInfoOrNull = value;
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
