using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Chapter3.Library;

namespace Chapter3.Examples
{
    [Description("Object address with ExplicitLayout trick")]
    class ObjectAddressDemo
    {
        static void Main()
        {
            object obj = new SampleClass();
            Console.WriteLine($"Method1: {obj.GetAddress1():x16}");
            Console.WriteLine($"Method2: {obj.GetAddress2():x16}");
            Console.WriteLine($"Method3: {obj.GetAddress3():x16}");
            Console.WriteLine($"Method4: {obj.GetAddress4():x16}");
        }

        class SampleClass
        {
            private int _field;
        }
    }
}
