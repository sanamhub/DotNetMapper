namespace DotNetMapper.Tests;

// Every behaviour test runs both paths. These prove the direct call really is the generated
// code, otherwise a broken generator would leave the suite green on the runtime path twice.
public sealed class PathTests
{
    [Fact]
    public void Direct_call_runs_generated_code()
    {
        _ = Mapper.Map<TracingSource, TracingTarget>(new TracingSource { Id = 1 });

        Assert.DoesNotContain("lambda_method", TracingSource.LastStack, StringComparison.Ordinal);
    }

    [Fact]
    public void Generic_call_runs_the_compiled_delegate()
    {
        _ = RuntimePath.Map<TracingSource, TracingTarget>(new TracingSource { Id = 1 });

        Assert.Contains("lambda_method", TracingSource.LastStack, StringComparison.Ordinal);
    }
}
