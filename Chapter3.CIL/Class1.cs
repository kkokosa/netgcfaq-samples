using System;
using System.Runtime.CompilerServices;

namespace Chapter3.CIL
{
    public static class Class1
    {
        [MethodImpl(MethodImplOptions.ForwardRef)]
        public static extern Int64 GetAddress(this object obj);
    }
}
