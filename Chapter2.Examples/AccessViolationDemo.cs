#define PINVOKE_EXAMPLE

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Chapter1
{
    [Description("Access violation with refs")]
    class AccessViolationDemoRefs
    {
        static void Main()
        {
            int local = 0;
            ref var refLocal = ref Unsafe.Add(ref local, int.MaxValue);
            refLocal = 1;
            // --- or ---
            // Unsafe.ReadUnaligned<int>(ref Unsafe.Add(ref local, int.MaxValue));
        }
    }

    [Description("Access violation with raw poiner")]
    class AccessViolationDemoPointer
    {
        static int Main()
        {
            unsafe
            {
                var r = *((int*)0x1_0000 + 1);
                return r;
            }
        }
    }

    /// <summary>
    /// https://twitter.com/KooKiz/status/1242730131984089088
    /// </summary>
    [Description("Access violation with ExplicitLayout")]
    class AccessViolationDemoExplicitLayout
    {
        [StructLayout(LayoutKind.Explicit)]
        struct Pointer
        {
            [FieldOffset(0)] public object[] Reference;
            [FieldOffset(0)] public long[] Address;
        }

        static void Main()
        {
            var pointer = new Pointer() {Address = new[] { long.MaxValue }};
            pointer.Reference[0].ToString();
        }
    }

    [Description("Access violation with unmanaged P/Invoke call")]
    class AccessViolationDemoUnmanaged
    {
        [DllImport("Chapter2.Unmanaged.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int fnUnmanaged(int argument);

        static void Main() =>
            Console.WriteLine(fnUnmanaged(10));
    }

    [Description("No access violation with Marshal (but Internal CLR error)")]
    class AccessViolationDemoMarshal
    {
        static void Main() =>
            Marshal.StructureToPtr(new Point(), new IntPtr(0x1_0000 + 1), false);
    }
}
