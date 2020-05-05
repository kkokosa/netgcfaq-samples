using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Chapter3.Core;
using Microsoft.Diagnostics.Runtime;
using ObjectLayoutInspector;

namespace Chapter6.Examples
{
    [Description("ClrMD demo")]
    class ClrMdDemo
    {
        public static void Main(string[] args)
        {
            var st1 = new SomeType();
            var st2 = new SomeType { S = "Hello!" };

            var pid = Process.GetCurrentProcess().Id;
            using var target = DataTarget.AttachToProcess(pid, 3000, AttachFlag.Passive);
            var runtime = target.ClrVersions[0].CreateRuntime();
            var heap = runtime.Heap;
            Console.WriteLine($"CanWalkHeap: {heap.CanWalkHeap}");
            ulong st1size = ObjSize(heap, (ulong)st1.GetAddress1());
            Console.WriteLine($"ClrMd st1 size: {st1size}");
            ulong st2size = ObjSize(heap, (ulong)st2.GetAddress1());
            Console.WriteLine($"ClrMd st2 size: {st2size}");

            // For reference types it will return size of the reference...
            var stsize = Unsafe.SizeOf<SomeType>();
            Console.WriteLine($"Unsafe st size: {stsize}");

            // Object Layout Inspector returns managed part of the object,
            // so we only need to add two pointer-sizes (header, MT)
            var stsize2 = InspectorHelper.GetSizeOfReferenceTypeInstance(typeof(SomeType));
            Console.WriteLine($"OLI st size: {stsize2}");

            var fields =
                typeof(SomeType).GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                Console.WriteLine(field.FieldType.IsClass);
            }
        }

        private static ulong ObjSize(ClrHeap heap, ulong address)
        {
            Stack<ulong> eval = new Stack<ulong>();

            // ObjectSet is memory-efficient HashSet<ulong>
            ObjectSet considered = new ObjectSet(heap);
            ulong size = 0;
            eval.Push(address);
            while (eval.Count > 0)
            {
                address = eval.Pop();
                if (considered.Contains(address))
                    continue;
                considered.Add(address);
                ClrType type = heap.GetObjectType(address);
                if (type == null) // Heap-corruption :(
                    continue;
                size += type.GetSize(address);
                type.EnumerateRefsOfObject(address, (child, _) =>
                {
                    if (child != 0 && !considered.Contains(child))
                        eval.Push(child);
                });
            }
            return size;
        }

    }

    class SomeType
    {
        public int F1;
        public int F2;
        public string S;
    }
}
