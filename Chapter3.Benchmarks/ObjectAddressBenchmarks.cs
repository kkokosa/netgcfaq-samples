using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Chapter3.Library;

namespace Chapter3.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class ObjectAddressBenchmarks
    {
        private object obj = new SampleClass();

        [Benchmark]
        public long GetAddress1() => obj.GetAddress1();
        [Benchmark]
        public long GetAddress2() => obj.GetAddress3();
        [Benchmark]
        public long GetAddress3() => obj.GetAddress4();


        class SampleClass
        {
            private int _field;
        }
    }
}
