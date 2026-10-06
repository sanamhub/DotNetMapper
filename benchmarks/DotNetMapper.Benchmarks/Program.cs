using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace DotNetMapper.Benchmarks;

internal static class Program
{
    private static void Main(string[] args)
    {
        // JoinSummary prints one table across all classes, which makes the tiers comparable at a
        // glance instead of scattered over several summaries. The Baseline column is not on by
        // default; without it the export says which row is the baseline only by its 1.00 ratio.
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(
                args,
                DefaultConfig.Instance
                    .WithOptions(ConfigOptions.JoinSummary)
                    .AddColumn(BaselineColumn.Default));
    }
}
