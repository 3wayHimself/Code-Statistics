using System;
using System.Reflection;

namespace CodeStatistics{
    static class Program{
        public static string Version { get; } = typeof(Program).GetTypeInfo().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private static void Main(string[] args){
            Console.Title = "Code Statistics "+Version;
            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
