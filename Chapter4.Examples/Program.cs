using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Chapter4.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("EvilType",
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);

            var coreLibAssembly = typeof(string).Assembly;
            var genericByRefLikeType = coreLibAssembly.GetType("System.ByReference`1");
            var constructedByRefLikeType = genericByRefLikeType.MakeGenericType(typeof(string));

            typeBuilder.DefineField("_byRefLikeInstanceField", 
                                    constructedByRefLikeType, 
                                    FieldAttributes.Public);

            try
            {
                TypeInfo objectTypeInfo = typeBuilder.CreateTypeInfo();

                var evilType = objectTypeInfo.AsType();
                var evilTypeInstance = Activator.CreateInstance(evilType);
            }
            catch (TypeLoadException ex)
            {
                // We do expect it with the message:
                //  A ByRef-like type cannot be used as the type for an instance field in a non-ByRef-like type
                Console.WriteLine($"Expected exception {ex}");
            }
        }
    }
}
