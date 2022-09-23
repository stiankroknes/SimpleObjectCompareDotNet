using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace SimpleObjectComparerDotNet.Benchmarks;

[MemoryDiagnoser(displayGenColumns: false)]
[DisassemblyDiagnoser]
[HideColumns("Error", "StdDev", "Median", "RatioSD")]
public class Program
{
    public static void Main(string[] args) => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
}
