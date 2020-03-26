using System;
using BenchmarkDotNet.Running;

namespace Chapter3.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var r= BenchmarkRunner.Run<ObjectAddressBenchmarks>();
        }
    }
}
