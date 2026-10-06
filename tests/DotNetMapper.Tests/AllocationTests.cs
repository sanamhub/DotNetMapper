using System.Runtime.CompilerServices;

namespace DotNetMapper.Tests;

public sealed class AllocationTests
{
    [Fact]
    public void Struct_to_struct_mapping_allocates_nothing()
    {
        var input = new AllocStructSource { Id = 1 };

        for (int i = 0; i < 100; i++)
        {
            _ = Mapper.Map<AllocStructSource, AllocStructTarget>(input);
            _ = RuntimePath.Map<AllocStructSource, AllocStructTarget>(input);
        }

        Assert.Equal(0, Measure(() => Mapper.Map<AllocStructSource, AllocStructTarget>(input), 1000));
        Assert.Equal(0, Measure(() => RuntimePath.Map<AllocStructSource, AllocStructTarget>(input), 1000));
    }

    [Fact]
    public void Runtime_class_mapping_allocates_only_the_result()
    {
        var input = new AllocClassSource { Id = 1, Name = "n" };

        _ = RuntimePath.Map<AllocClassSource, AllocClassTarget>(input);

        long single = Measure(() => RuntimePath.Map<AllocClassSource, AllocClassTarget>(input), 1);
        long total = Measure(() => RuntimePath.Map<AllocClassSource, AllocClassTarget>(input), 1000);

        Assert.True(total <= single * 1000, $"allocated {total} for 1000 calls, single result {single}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static long Measure(Action action, int count)
    {
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < count; i++)
        {
            action();
        }

        return GC.GetAllocatedBytesForCurrentThread() - before;
    }
}
