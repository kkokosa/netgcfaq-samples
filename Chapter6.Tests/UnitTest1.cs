using System;
using System.Text;
using JetBrains.dotMemoryUnit;
using Xunit;
using Xunit.Abstractions;

namespace Chapter6.Tests
{
    [DotMemoryUnit(FailIfRunWithoutSupport = false)]
    public class UnitTest1
    {
        public UnitTest1(ITestOutputHelper outputHelper)
        {
            DotMemoryUnitTestOutput.SetOutputMethod(
                message => outputHelper.WriteLine(message));
        }

        [AssertTraffic(AllocatedObjectsCount = 0)]
        [Fact]
        public void Test1()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Hello");
        }
    }
}
