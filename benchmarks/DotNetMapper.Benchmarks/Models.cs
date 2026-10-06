using System.Runtime.CompilerServices;

namespace DotNetMapper.Benchmarks;

public enum Color
{
    None,
    Red,
    Green,
    Blue,
}

public sealed class Source
{
    public int Id { get; set; }
    public long Count { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime When { get; set; }
    public decimal Amount { get; set; }
    public Guid Key { get; set; }
    public bool Flag { get; set; }
    public Color Color { get; set; }
    public List<string> Tags { get; set; } = [];
}

public sealed class Destination
{
    public int Id { get; set; }
    public long Count { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime When { get; set; }
    public decimal Amount { get; set; }
    public Guid Key { get; set; }
    public bool Flag { get; set; }
    public Color Color { get; set; }
    public List<string> Tags { get; set; } = [];
}

public sealed class Small
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class SmallDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

// A generic call site cannot be intercepted, so this always takes the runtime path.
internal static class RuntimePath
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TOut Map<TIn, TOut>(TIn input)
        where TOut : new()
        => Mapper.Map<TIn, TOut>(input);
}
