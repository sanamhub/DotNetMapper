using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Mapster;

namespace DotNetMapper.Benchmarks;

// One call per launch, ten launches. Measures the first-call cost: the interceptor is already
// inlined by the compiler, the runtime path pays one expression compile, and Mapster pays its
// config compilation on the first Adapt call.
[SimpleJob(RunStrategy.ColdStart, launchCount: 10, warmupCount: 0, iterationCount: 1)]
[MemoryDiagnoser]
public class ColdStartBenchmarks
{
    private Source _source = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _source = new Source
        {
            Id = 1,
            Count = 2,
            FirstName = "first",
            LastName = "last",
            When = new DateTime(2026, 1, 2, 3, 4, 5),
            Amount = 12.5m,
            Key = Guid.NewGuid(),
            Flag = true,
            Color = Color.Green,
            Tags = ["a", "b"],
        };
    }

    [Benchmark]
    public Destination DotNetMapper() => Mapper.Map<Source, Destination>(_source);

    [Benchmark]
    public Destination DotNetMapper_Runtime() => RuntimePath.Map<Source, Destination>(_source);

    [Benchmark]
    public Destination DotNetMapper_1_0_2() => LegacyMapper.Map<Source, Destination>(_source);

    [Benchmark]
    public Destination Mapperly() => MapperlyMapper.Map(_source);

    [Benchmark]
    public Destination Mapster() => _source.Adapt<Destination>();
}
