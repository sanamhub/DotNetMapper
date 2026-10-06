using AgileObjects.AgileMapper;
using BenchmarkDotNet.Attributes;
using Mapster;
using Nelibur.ObjectMapper;

namespace DotNetMapper.Benchmarks;

// Ten properties, one of them a List<string>. DotNetMapper and Mapperly copy the list reference.
// AutoMapper, Mapster, TinyMapper and AgileMapper copy the list itself by default, which shows up
// in the Allocated column. SmallThroughputBenchmarks has no collection and compares like for like.
[MemoryDiagnoser]
public class ThroughputBenchmarks
{
    private Source _source = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _source = Competitors.NewSource();
        Competitors.Warm();
    }

    [Benchmark(Baseline = true)]
    public Destination Manual() => new()
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
    public Destination DotNetMapper() => global::DotNetMapper.Mapper.Map<Source, Destination>(_source);

    [Benchmark]
    public Destination DotNetMapper_Runtime() => RuntimePath.Map<Source, Destination>(_source);

    [Benchmark]
    public Destination DotNetMapper_1_0_2() => LegacyMapper.Map<Source, Destination>(_source);

    [Benchmark]
    public Destination Mapperly() => MapperlyMapper.Map(_source);

    [Benchmark]
    public Destination Mapster() => _source.Adapt<Destination>();

    [Benchmark]
    public Destination AutoMapper() => Competitors.AutoMapper.Map<Destination>(_source);

    [Benchmark]
    public Destination TinyMapper() => Nelibur.ObjectMapper.TinyMapper.Map<Destination>(_source);

    [Benchmark]
    public Destination AgileMapper() => AgileObjects.AgileMapper.Mapper.Map(_source).ToANew<Destination>();
}

// Two properties, no collection: every library does exactly the same work.
[MemoryDiagnoser]
public class SmallThroughputBenchmarks
{
    private Small _small = new();

    [GlobalSetup]
    public void GlobalSetup()
    {
        _small = new Small { Id = 7, Name = "small" };
        Competitors.Warm();
    }

    [Benchmark(Baseline = true)]
    public SmallDto Manual() => new() { Id = _small.Id, Name = _small.Name };

    [Benchmark]
    public SmallDto DotNetMapper() => global::DotNetMapper.Mapper.Map<Small, SmallDto>(_small);

    [Benchmark]
    public SmallDto DotNetMapper_Runtime() => RuntimePath.Map<Small, SmallDto>(_small);

    [Benchmark]
    public SmallDto DotNetMapper_1_0_2() => LegacyMapper.Map<Small, SmallDto>(_small);

    [Benchmark]
    public SmallDto Mapperly() => MapperlyMapper.Map(_small);

    [Benchmark]
    public SmallDto Mapster() => _small.Adapt<SmallDto>();

    [Benchmark]
    public SmallDto AutoMapper() => Competitors.AutoMapper.Map<SmallDto>(_small);

    [Benchmark]
    public SmallDto TinyMapper() => Nelibur.ObjectMapper.TinyMapper.Map<SmallDto>(_small);

    [Benchmark]
    public SmallDto AgileMapper() => AgileObjects.AgileMapper.Mapper.Map(_small).ToANew<SmallDto>();
}
