using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Benchmarks;

[MemoryDiagnoser]
public class Program {
    private static void Main(string[] args) {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
