﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CppMemoryVisualizer.Models
{
    sealed class LocalVariable
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

        public LocalVariable(string name, bool isArgument)
        {
            Debug.Assert(name != null);

            mName = name;
            mbArgument = isArgument;
        }
    }
}
