using System;
using BenchmarkDotNet.Running;

namespace Chapter3.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            var r = BenchmarkRunner.Run<ObjectAddressBenchmarks>();
        }
    }
}
