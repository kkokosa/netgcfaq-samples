using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Chapter3.CIL;
using Chapter3.Core;

namespace Chapter3.Tests
{
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class ObjectAddressBenchmarks
    {
        private object obj = new SampleClass();

        [Benchmark]
        public long GetAddress1() => obj.GetAddress1();
        //[Benchmark]
        public long GetAddress2() => obj.GetAddress2();
        //[Benchmark]
        public long GetAddress3() => obj.GetAddress3();
        //[Benchmark]
        public long GetAddress4() => obj.GetAddress4();
        [Benchmark]
        public long GetAddress() => obj.GetAddress();

        class SampleClass
        {
            private int _field;
        }
    }
}
