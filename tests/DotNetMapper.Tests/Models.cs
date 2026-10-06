using System.Collections.ObjectModel;

namespace DotNetMapper.Tests;

public enum Color
{
    None,
    Red,
    Green,
}

// Scenario 1: same-name, same-type properties.
public sealed class BasicSource
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime When { get; set; }
    public decimal Amount { get; set; }
    public Guid Key { get; set; }
    public bool Flag { get; set; }
    public Color Color { get; set; }
    public Collection<string> Tags { get; init; } = [];
    public int? Optional { get; set; }
}

public sealed class BasicTarget
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime When { get; set; }
    public decimal Amount { get; set; }
    public Guid Key { get; set; }
    public bool Flag { get; set; }
    public Color Color { get; set; }
    public Collection<string> Tags { get; init; } = [];
    public int? Optional { get; set; }
}

// Scenario 2: name match is case sensitive.
public sealed class CaseSource
{
    public string Name { get; set; } = "";
}

public sealed class CaseTarget
{
    public string name { get; set; } = "";
}

// Scenario 3: type mismatch is not copied.
public sealed class TypeMismatchSource
{
    public int Number { get; set; }
    public int ToNullable { get; set; }
    public string Text { get; set; } = "";
}

public sealed class TypeMismatchTarget
{
    public long Number { get; set; }
    public int? ToNullable { get; set; }
    public object? Text { get; set; }
}

// Scenario 4: a source property without a public getter is not read. The shapes below are
// deliberately wrong; they exist to prove the mapper does not read them.
public sealed class PrivateGetterSource
{
    private int _value;

#pragma warning disable CA1044 // Getter is private on purpose: the test needs a non-public getter.
    public int Value
    {
        private get => _value;
        set => _value = value;
    }
#pragma warning restore CA1044
}

public sealed class WriteOnlySource
{
#pragma warning disable CA1044 // Write-only on purpose: the test needs a property with no getter.
#pragma warning disable CA1822 // The setter intentionally ignores the value for this test model.
    public int Value
    {
        set => _ = value;
    }
#pragma warning restore CA1822
#pragma warning restore CA1044
}

public sealed class PlainTarget
{
    public int Value { get; set; }
}

// Scenario 5: targets without a public setter are not written.
public sealed class PrivateSetterTarget
{
    public int Value { get; private set; }
}

public sealed class InternalSetterTarget
{
    public int Value { get; internal set; }
}

public sealed class GetterOnlyTarget
{
    public int Value { get; }
}

// Scenario 6: init setters are written.
public sealed class InitTarget
{
    public int Value { get; init; }
}

// Scenario 7: a record with init properties and a parameterless constructor.
public sealed record RecordTarget
{
    public RecordTarget()
    {
    }

    public int Id { get; init; }
    public string Name { get; init; } = "";
}

// Scenario 8: value type combinations.
public record struct StructSource
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public record struct StructTarget
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public sealed class SmallClassSource
{
    public int Value { get; set; }
}

public record struct SmallStructTarget
{
    public int Value { get; set; }
}

// Scenario 9: inherited properties are copied.
public class BaseSource
{
    public int BaseId { get; set; }
}

public sealed class DerivedSource : BaseSource
{
    public int DerivedId { get; set; }
}

public class BaseTarget
{
    public int BaseId { get; set; }
}

public sealed class DerivedTarget : BaseTarget
{
    public int DerivedId { get; set; }
}

// Scenario 10: a hidden property, most-derived declaration wins.
public class HiddenBaseSource
{
    public string Name { get; set; } = "base";
}

public sealed class HiddenDerivedSource : HiddenBaseSource
{
    public new string Name { get; set; } = "derived";
}

public sealed class HiddenTarget
{
    public string Name { get; set; } = "";
}

// Scenario 11: indexers are ignored.
public sealed class IndexerSource
{
    private readonly Dictionary<int, string> _items = [];

    public string this[int i]
    {
        get => _items[i];
        set => _items[i] = value;
    }
}

public sealed class IndexerTarget
{
    private readonly Dictionary<int, string> _items = [];

    public string this[int i]
    {
        get => _items[i];
        set => _items[i] = value;
    }
}

// Scenario 12: static properties are ignored.
public sealed class StaticSource
{
    public static int StaticProp { get; set; }

    public int InstanceProp { get; set; }
}

public sealed class StaticTarget
{
    public static int StaticProp { get; set; }

    public int InstanceProp { get; set; }
}

// Scenario 16: no matching properties.
public sealed class NoMatchSource
{
    public int A { get; set; }
}

public sealed class NoMatchTarget
{
    public int B { get; set; }
}

// Threading test: a fresh pair used nowhere else.
public sealed class FreshA
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class FreshB
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

// Allocation test: struct pair and class pair.
public record struct AllocStructSource
{
    public int Id { get; set; }
}

public record struct AllocStructTarget
{
    public int Id { get; set; }
}

public sealed class AllocClassSource
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public sealed class AllocClassTarget
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
