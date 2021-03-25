using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Chapter3.Core
{
    public static class ObjectExtensions
    {
        public static unsafe long GetAddress1(this object obj)
        {
            var ptr = *(IntPtr*)Unsafe.AsPointer(ref obj);
            return ptr.ToInt64();
        }

        public static unsafe long GetAddress2(this object obj)
        {
            TypedReference tr = __makeref(obj);
            IntPtr ptr = **(IntPtr**)(&tr);
            return ptr.ToInt64();
        }

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

        public static long GetAddress3(this object @obj)
        {
            var pointer = new Pointer() { Reference = @obj };
            return pointer.Address;
        }

        public static long GetAddress4(this object @obj) => _method(@obj);

        public static long GetAddress5(this object obj)
        {
            GCHandle handle = GCHandle.Alloc(obj, GCHandleType.Pinned);
            // AddrOfPinnedObject retrieves the address of object data in a Pinned handle, which is:
            // For arrays, this method returns the address of the first element. For strings, this
            // method returns the address of the first character
            return handle.AddrOfPinnedObject().ToInt64();
        }

        private static Func<object, long> _method;
        static ObjectExtensions()
        {
            var method = new DynamicMethod("DynamicInvoke", typeof(long), new[] { typeof(object) });
            var generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ret);
            _method = (Func<object, long>)method.CreateDelegate(typeof(Func<object, long>));
        }
    }
}
