using System;

namespace ConsoleSimulator.Core
{
    public static class RandomEnum
    {
        private static readonly Random _random = new Random(Environment.TickCount);

        public static T Of<T>() where T : Enum
        {
            Array enumValues = Enum.GetValues(typeof(T));
            return (T)enumValues.GetValue(_random.Next(enumValues.Length))!;
        }
    }
}
