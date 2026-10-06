namespace DotNetMapper.Tests;

public sealed class ThreadingTests
{
    [Fact]
    public async Task Concurrent_first_calls_all_map_correctly()
    {
        const int count = 32;
        using var barrier = new Barrier(count);
        var tasks = new Task<FreshB>[count];

        for (int i = 0; i < count; i++)
        {
            int id = i;
            tasks[i] = Task.Run(() =>
            {
                barrier.SignalAndWait();
                return RuntimePath.Map<FreshA, FreshB>(new FreshA { Id = id, Name = $"n{id}" });
            });
        }

        FreshB[] results = await Task.WhenAll(tasks);
        for (int i = 0; i < count; i++)
        {
            Assert.Equal(i, results[i].Id);
            Assert.Equal($"n{i}", results[i].Name);
        }
    }
}
