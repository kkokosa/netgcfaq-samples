using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Chapter3.Library
{
    public static class ObjectExtensions
    {
        // This will not compile as it throws:
        // System.Reflection.ReflectionTypeLoadException: Unable to load one or more of the requested types.
        // Could not load type 'Pointer' from assembly '...' because it contains an object field at offset 0
        // that is incorrectly aligned or overlapped by a non - object field.
        //
        // [StructLayout(LayoutKind.Explicit)]
        // struct Pointer
        // {
        //    [FieldOffset(0)] public object Reference;
        //    [FieldOffset(0)] public long Address;
        // }
        [StructLayout(LayoutKind.Explicit)]
        struct Pointer
        {
            [FieldOffset(0)] private object[] _reference;
            [FieldOffset(0)] private long[] _address;

            public object Reference { set => _reference = new[] { value }; }
            public long Address => _address[0];
        }

        public static long GetAddress1(this object @obj)
        {
            var pointer = new Pointer() { Reference = @obj };
            return pointer.Address;
        }

        public static long GetAddress2(this object obj)
        {
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            // AddrOfPinnedObject retrieves the address of object data in a Pinned handle, which is:
            // For arrays, this method returns the address of the first element. For strings, this
            // method returns the address of the first character
            return handle.AddrOfPinnedObject().ToInt64();
        }

        public static unsafe long GetAddress3(this object obj)
        {
            TypedReference tr = __makeref(obj);
            IntPtr ptr = **(IntPtr**)(&tr);
            return ptr.ToInt64();
        }

        public static unsafe long GetAddress4(this object obj)
        {
            var ptr = *(IntPtr*)Unsafe.AsPointer(ref obj);
            return ptr.ToInt64();
        }
    }

}
