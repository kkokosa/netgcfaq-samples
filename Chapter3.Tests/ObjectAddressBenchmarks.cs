using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Chapter3.CIL;
using Chapter3.Core;

namespace Chapter3.Tests
{
    /*
    If it does not run, execute manually inside Chapter3.Tests folder:
    > dotnet build -c Release  --no-restore --no-dependencies /p:UseSharedCompilation=false /p:BuildInParallel=false /m:1

    |      Method |      Mean |     Error |    StdDev |
    |------------ |----------:|----------:|----------:|
    |  GetAddress | 0.8087 ns | 0.0563 ns | 0.0526 ns |
    | GetAddress1 | 0.0350 ns | 0.0291 ns | 0.0272 ns |
    | GetAddress2 | 1.3670 ns | 0.0462 ns | 0.0432 ns |
    | GetAddress3 | 9.0657 ns | 0.2098 ns | 0.3992 ns |
    | GetAddress4 | 2.7241 ns | 0.0792 ns | 0.0740 ns |
     */
    [DisassemblyDiagnoser(printIL: true, printAsm: true, printPrologAndEpilog: true)]
    [SimpleJob(RuntimeMoniker.NetCoreApp31)]
    public class ObjectAddressBenchmarks
    {
        private object obj = new SampleClass();

        [Benchmark]
        public long GetAddress() => obj.GetAddress();
        [Benchmark]
        public long GetAddress1() => obj.GetAddress1();
        [Benchmark]
        public long GetAddress2() => obj.GetAddress2();
        [Benchmark]
        public long GetAddress3() => obj.GetAddress3();
        [Benchmark]
        public long GetAddress4() => obj.GetAddress4();


        class SampleClass
        {
            private int _field;
        }
    }
}
