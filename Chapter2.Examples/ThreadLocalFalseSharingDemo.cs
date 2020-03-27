using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Chapter3.Core;
using Microsoft.Diagnostics.Runtime;

namespace Chapter2.Examples
{
    [Description("False sharing with TLS")]
    class ThreadLocalFalseSharingDemo
    {
        private ThreadLocal<SomeTLSData> _threadLocal = new ThreadLocal<SomeTLSData>(() =>
            new SomeTLSData());

        static void Main()
        {
            Console.WriteLine(GCSettings.IsServerGC ? "Server GC" : "Workstation GC");

            var runner = new ThreadLocalFalseSharingDemo();
            var threads = Enumerable.Range(0, 4).Select(x => new Thread(runner.Run)).ToList();
            threads.ForEach(t => t.Start());
            threads.ForEach(t => t.Join());

            //DataTarget dataTarget =
            //    DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 100, AttachFlag.Passive);
            //foreach (ClrInfo version in dataTarget.ClrVersions)
            //{
            //    Console.WriteLine("Found CLR Version: " + version.Version);
            //    ClrRuntime runtime = version.CreateRuntime();
            //    foreach (var region in runtime.EnumerateMemoryRegions())
            //    {
            //        var gcType = region.Type == ClrMemoryRegionType.GCSegment ? region.GCSegmentType.ToString() : string.Empty;
            //        Console.WriteLine($"{region.Address}-{region.Address + region.Size} {region.Type} {gcType}");
            //    }
            //}
        }

        /// <summary>
        /// It is not easy to compact object-by-object even for TLS data because TLS 
        /// </summary>
        public void Run()
        {
            var threadId = Thread.CurrentThread.ManagedThreadId;
            var prevAddress = 0L;
            var sum = 0L;
            while (true)
            {
                var obj = _threadLocal.Value;
                var address = obj.GetAddress1();
                var gen = GC.GetGeneration(obj);
                if (address != prevAddress)
                {
                    Console.WriteLine($"[TID {threadId}] {prevAddress:x16}->{address:x16} ({address-prevAddress}), gen {gen}");
                    prevAddress = address;
                }
                if (gen < 2)
                    sum += new SomeData().GetData(); // Allocate and consume!
                var allocated = GC.GetAllocatedBytesForCurrentThread();
                if (allocated % 0x1_0000 == 0 && gen < 2)
                {
                    GC.Collect(2, GCCollectionMode.Forced, true, true);
                }
            }
        }

        class SomeTLSData
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public int GetData() => 1;
        }
        class SomeData
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            public int GetData() => 1;
        }
    }
}
