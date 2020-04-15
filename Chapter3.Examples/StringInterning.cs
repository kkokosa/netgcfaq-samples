using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Chapter3.Examples
{
    [Description("String interning examples")]
    class StringInterning
    {
        private static string GlobalString = "Hello world!";
        static void Main()
        {
            string str1 = "Hello world!";
            Console.WriteLine($"str1: {object.ReferenceEquals(GlobalString, str1)}");

            string str2 = "Hello" + " world!"; // This is optimized by compiler as ldstr "Hello world!"
            Console.WriteLine($"str2: {object.ReferenceEquals(GlobalString, str2)}");

            string str3 = "H" + "e" + "l" + "l" + "o" + " " + "w" + "o" + "r" + "l" + "d" + "!"; // This is still optimized by compiler as ldstr "Hello world!"
            Console.WriteLine($"str3: {object.ReferenceEquals(GlobalString, str3)}");

            string hello = "Hello";
            string world = " world!";

            string str4 = hello + world; // This calls System.String::Concat(string, string) so is not known at compile time
            Console.WriteLine($"str4: {object.ReferenceEquals(GlobalString, str4)}"); 

            string str5 = $"{hello}{world}"; // Same for string interpolation
            Console.WriteLine($"str5: {object.ReferenceEquals(GlobalString, str5)}");

            string str6 = string.IsInterned("Hello world!"); 
            Console.WriteLine($"str6: {object.ReferenceEquals(GlobalString, str6)}");

            string str7 = string.IsInterned(hello + world);
            Console.WriteLine($"str7: {object.ReferenceEquals(GlobalString, str7)}");

            string str8 = string.IsInterned("h");
            Console.WriteLine($"str8: {!(str8 is null)}");
        }
    }
}
