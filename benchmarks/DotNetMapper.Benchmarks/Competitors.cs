using AutoMapper;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using Nelibur.ObjectMapper;

namespace DotNetMapper.Benchmarks;

// Every library is set up once with its defaults, the way a reader would use it. Their first-call
// cost is measured by ColdStartBenchmarks, not here.
internal static class Competitors
{
    public static IMapper AutoMapper { get; } = new MapperConfiguration(
        cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<Small, SmallDto>();
        },
        NullLoggerFactory.Instance).CreateMapper();

    public static void Warm()
    {
        TinyMapper.Bind<Source, Destination>();
        TinyMapper.Bind<Small, SmallDto>();
        TypeAdapterConfig.GlobalSettings.Compile();
        _ = AutoMapper;
    }

    public static Source NewSource() => new()
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
