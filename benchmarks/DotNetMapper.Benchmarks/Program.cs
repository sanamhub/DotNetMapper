using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace DotNetMapper.Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        // No JoinSummary: each class has its own Manual baseline, and a joined table computes every
        // ratio against one of them. The Baseline column is not on by default; without it the
        // export says which row is the baseline only by its 1.00 ratio.
        // AgileObjects.AgileMapper 1.8.1 ships a non-optimised build, which BenchmarkDotNet refuses
        // to run by default. Its numbers are what users of that package get, so the check is off.
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(
                args,
                DefaultConfig.Instance
                    .WithOptions(ConfigOptions.DisableOptimizationsValidator)
                    .AddColumn(BaselineColumn.Default));
    }
}
