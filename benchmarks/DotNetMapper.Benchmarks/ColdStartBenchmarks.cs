using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using AgileObjects.AgileMapper;
using AutoMapper;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using Nelibur.ObjectMapper;

namespace DotNetMapper.Benchmarks;

// One call per launch, ten launches. Measures the first-call cost, including whatever setup each
// library needs before it can map: the generated code needs none, the runtime path compiles one
// expression, and the reflection-based libraries build and compile their plans.
[SimpleJob(RunStrategy.ColdStart, launchCount: 10, warmupCount: 0, iterationCount: 1)]
[MemoryDiagnoser]
public class ColdStartBenchmarks
{
    private Source _source = new();

    [GlobalSetup]
    public void GlobalSetup() => _source = Competitors.NewSource();

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
    public Destination AutoMapper() => new MapperConfiguration(
        cfg => cfg.CreateMap<Source, Destination>(),
        NullLoggerFactory.Instance).CreateMapper().Map<Destination>(_source);

    [Benchmark]
    public Destination TinyMapper()
    {
        Nelibur.ObjectMapper.TinyMapper.Bind<Source, Destination>();
        return Nelibur.ObjectMapper.TinyMapper.Map<Destination>(_source);
    }

    [Benchmark]
    public Destination AgileMapper() => AgileObjects.AgileMapper.Mapper.Map(_source).ToANew<Destination>();
}
