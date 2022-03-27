using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks;

[MemoryDiagnoser]
public class Program {
    internal const string Value = "(Minecraft.Contains(Mine) || ((30 > 30 && Genshing == Genshin) || (100 > 101 && (Englisch == English))))";
    
    private static void Main(string[] args) {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
