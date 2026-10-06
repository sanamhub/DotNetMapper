using BenchmarkDotNet.Attributes;
using Mapster;

namespace DotNetMapper.Benchmarks;

[MemoryDiagnoser]
public class ThroughputBenchmarks
{
    private Source _source = new();
    private Small _small = new();

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

        _small = new Small { Id = 7, Name = "small" };

        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [Benchmark(Baseline = true)]
    public Destination Manual() => new Destination
    {
        Id = _source.Id,
        Count = _source.Count,
        FirstName = _source.FirstName,
        LastName = _source.LastName,
        When = _source.When,
        Amount = _source.Amount,
        Key = _source.Key,
        Flag = _source.Flag,
        Color = _source.Color,
        Tags = _source.Tags,
    };

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

[MemoryDiagnoser]
public class SmallThroughputBenchmarks
{
    private Small _small = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _small = new Small { Id = 7, Name = "small" };
        TypeAdapterConfig.GlobalSettings.Compile();
    }

    [Benchmark(Baseline = true)]
    public SmallDto Manual() => new SmallDto { Id = _small.Id, Name = _small.Name };

    [Benchmark]
    public SmallDto DotNetMapper() => Mapper.Map<Small, SmallDto>(_small);

    [Benchmark]
    public SmallDto DotNetMapper_Runtime() => RuntimePath.Map<Small, SmallDto>(_small);

    [Benchmark]
    public SmallDto DotNetMapper_1_0_2() => LegacyMapper.Map<Small, SmallDto>(_small);

    [Benchmark]
    public SmallDto Mapperly() => MapperlyMapper.Map(_small);

    [Benchmark]
    public SmallDto Mapster() => _small.Adapt<SmallDto>();
}
