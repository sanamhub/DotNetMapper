using System.Diagnostics.CodeAnalysis;
using DotNetMapper;

var classResult = Mapper.Map<ClassSource, ClassTarget>(new ClassSource { Id = 1, Name = "c" });
if (classResult.Id != 1 || classResult.Name != "c")
{
    Console.Error.WriteLine("FAIL: class");
    return 1;
}

var structResult = Mapper.Map<StructSource, StructTarget>(new StructSource { Id = 2 });
if (structResult.Id != 2)
{
    Console.Error.WriteLine("FAIL: struct");
    return 1;
}

var recordResult = Mapper.Map<RecordSource, RecordTarget>(new RecordSource(3, "r"));
if (recordResult.Id != 3 || recordResult.Name != "r")
{
    Console.Error.WriteLine("FAIL: record");
    return 1;
}

var inherited = Mapper.Map<DerivedSource, DerivedTarget>(new DerivedSource { BaseId = 4, DerivedId = 5 });
if (inherited.BaseId != 4 || inherited.DerivedId != 5)
{
    Console.Error.WriteLine("FAIL: inherited");
    return 1;
}

var runtime = MapRuntime<ClassSource, ClassTarget>(new ClassSource { Id = 6, Name = "rt" });
if (runtime.Id != 6 || runtime.Name != "rt")
{
    Console.Error.WriteLine("FAIL: runtime");
    return 1;
}

Console.WriteLine("ok");
return 0;

static TOut MapRuntime<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TIn,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOut>(TIn input)
    where TOut : new()
    => Mapper.Map<TIn, TOut>(input);

sealed class ClassSource
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

sealed class ClassTarget
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

struct StructSource
{
    public int Id { get; set; }
}

struct StructTarget
{
    public int Id { get; set; }
}

sealed record RecordSource(int Id, string Name);

sealed record RecordTarget
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
}

class BaseSource
{
    public int BaseId { get; set; }
}

sealed class DerivedSource : BaseSource
{
    public int DerivedId { get; set; }
}

class BaseTarget
{
    public int BaseId { get; set; }
}

sealed class DerivedTarget : BaseTarget
{
    public int DerivedId { get; set; }
}
