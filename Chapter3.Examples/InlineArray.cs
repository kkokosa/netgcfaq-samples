using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Chapter3.Examples
{
    [Description("Inlining array instead of fixed size buffer")]
    class InlineArray
    {        
        public static int ExportedFun(int x, int y)
        {

        }

        public static void Main()
        {

        }

        unsafe struct StructWithFixedBuffer
        {
            private fixed int Buffer[123];
        }

        [StructLayout(LayoutKind.Explicit, Size = 128)]
        struct InlinedArray<T> where T : unmanaged
        {
            private T _items;

            public ref T this[int index]
            {
                get => ref Unsafe.Add(ref Unsafe.AsRef(this), index);
            }
        }
    }
}
