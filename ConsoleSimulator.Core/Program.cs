using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSimulator.Core
{
    class Program
    {
        private static readonly Random _random = new Random(Environment.TickCount);
        private static readonly List<object?> roots = new List<object?>(32_768);
        private static readonly double survivalRate = 0.9;

        static void Main(string[] args)
        {
            Console.WriteLine($"Current PID: {Process.GetCurrentProcess().Id}");
            Console.ReadLine();
            Task.Run(StatsTask);
            Task.Run(AllocatingTask);
            Task.Run(FreeingTask);
            Console.ReadLine();
        }

        private static void FreeingTask()
        {
            while (true)
            {
                lock (roots)
                {
                    for (int i = 0; i < roots.Count; ++i)
                    {
                        if (_random.NextDouble() >= survivalRate)
                            roots[i] = null;
                    }

                    int nullCount = roots.Count(x => !(x is null));
                    Console.CursorTop = 2;
                    Console.CursorLeft = 0;
                    Console.WriteLine($"Live objects ratio: {(double)nullCount/roots.Count:F2}");
                }
                Thread.Sleep(1000);
            }
        }

        private static void StatsTask()
        {
            while (true)
            {
                var allocated = GC.GetTotalAllocatedBytes(false);
                Console.CursorTop = 1;
                Console.CursorLeft = 0;
                Console.WriteLine($"Allocated bytes: {allocated:#,##0}");
                Thread.Sleep(100);
            }
        }

        private static void AllocatingTask()
        {
            while (true)
            {
                var obj = Allocate(true);
                lock (roots)
                {
                    bool slotReused = false;
                    for (int i = 0; i < roots.Count; ++i)
                    {
                        if (roots[i] == null)
                        {
                            roots[i] = obj;
                            slotReused = true;
                        }
                    }
                    if (!slotReused)
                        roots.Add(obj);
                }
                Thread.Sleep(250);
            }
        }

        static object Allocate(bool allowLOH)
        {
            int size = _random.Next(256, 80_000 + (allowLOH ? 100_000 : 0));
            return RandomEnum.Of<AllocationType>() switch
            {
                AllocationType.ByteArray => new byte[size],
                AllocationType.String => new string(' ', size),
                _ => new object()
            };
        }
    }
}
