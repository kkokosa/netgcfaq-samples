using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Chapter3.CIL;
using Chapter3.Core;

namespace Chapter3.Examples
{
    [Description("Various method to get an address of an object")]
    class ObjectAddressDemo
    {
        static void Main()
        {
            object obj = new SampleClass();
            Console.WriteLine($"Method1: {obj.GetAddress1():x16}");
            Console.WriteLine($"Method2: {obj.GetAddress2():x16}");
            Console.WriteLine($"Method3: {obj.GetAddress3():x16}");
            Console.WriteLine($"Method4: {obj.GetAddress4():x16}");
            // Won't work because of exception:
            // Exception: System.ArgumentException: Object contains non-primitive or non-blittable data. (Parameter 'value')
            //   at System.Runtime.InteropServices.GCHandle..ctor(Object value, GCHandleType type)
            // Console.WriteLine($"Method5: {obj.GetAddress5():x16}");
            Console.WriteLine($"Method0: {obj.GetAddress():x16}");
        }

        class SampleClass
        {
            private int _field;
        }
    }
}
