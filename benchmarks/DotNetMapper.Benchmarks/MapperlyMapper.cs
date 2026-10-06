using Riok.Mapperly.Abstractions;

namespace DotNetMapper.Benchmarks;

[Mapper]
internal static partial class MapperlyMapper
{
    public static partial Destination Map(Source source);

    public static partial SmallDto Map(Small source);
}
