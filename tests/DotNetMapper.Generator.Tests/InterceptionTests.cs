using Microsoft.CodeAnalysis;

namespace DotNetMapper.Generator.Tests;

public sealed class InterceptionTests
{
    private const string Input = """
        using DotNetMapper;

        namespace Demo
        {
            public class Input
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class Output
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class Program
            {
                public Output Go(Input x) => Mapper.Map<Input, Output>(x);
            }
        }
        """;

    [Fact]
    public void Concrete_call_is_intercepted_without_warnings()
    {
        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", Input)));

        Assert.Contains("InterceptsLocationAttribute", text, StringComparison.Ordinal);
        Assert.Contains("new global::Demo.Output", text, StringComparison.Ordinal);
        AssertNoDiagnostics(diagnostics);
    }

    [Fact]
    public void Two_calls_with_the_same_pair_share_one_method()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class Input
                {
                    public int Id { get; set; }
                }

                public class Output
                {
                    public int Id { get; set; }
                }

                public class Program
                {
                    public Output Go1(Input x) => Mapper.Map<Input, Output>(x);
                    public Output Go2(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal(2, CountOccurrences(text, "[global::System.Runtime.CompilerServices.InterceptsLocationAttribute("));
        Assert.Contains("Map0", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Map1", text, StringComparison.Ordinal);
        AssertNoDiagnostics(diagnostics);
    }

    [Fact]
    public void Two_pairs_produce_map0_and_map1()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class A { public int Id { get; set; } }
                public class B { public int Id { get; set; } }
                public class C { public string Name { get; set; } = ""; }
                public class D { public string Name { get; set; } = ""; }

                public class Program
                {
                    public B Go1(A x) => Mapper.Map<A, B>(x);
                    public D Go2(C x) => Mapper.Map<C, D>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Contains("Map0", text, StringComparison.Ordinal);
        Assert.Contains("Map1", text, StringComparison.Ordinal);
        AssertNoDiagnostics(diagnostics);
    }

    [Fact]
    public void Keyword_property_name_is_escaped_with_at_sign()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class Input { public string @class { get; set; } = ""; }
                public class Output { public string @class { get; set; } = ""; }

                public class Program
                {
                    public Output Go(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Contains("@class = inputObject.@class", text, StringComparison.Ordinal);
        AssertNoDiagnostics(diagnostics);
    }

    [Fact]
    public void Struct_input_emits_no_null_check()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public struct Input { public int Id { get; set; } }
                public class Output { public int Id { get; set; } }

                public class Program
                {
                    public Output Go(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.DoesNotContain("inputObject is null", text, StringComparison.Ordinal);
        AssertNoDiagnostics(diagnostics);
    }

    [Fact]
    public void Init_only_target_compiles()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class Input { public int Id { get; set; } }
                public class Output { public int Id { get; init; } }

                public class Program
                {
                    public Output Go(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Contains("@Id = inputObject.@Id", text, StringComparison.Ordinal);
        AssertNoDiagnostics(diagnostics);
    }

    [Fact]
    public void Nullable_annotation_mismatch_produces_no_warnings()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class Input { public string? Name { get; set; } }
                public class Output { public string Name { get; set; } = ""; }

                public class Program
                {
                    public Output Go(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Contains("@Name = inputObject.@Name", text, StringComparison.Ordinal);
        AssertNoDiagnostics(diagnostics);
    }

    [Fact]
    public void Output_is_deterministic_across_runs()
    {
        var compilation = GeneratorHarness.CreateCompilation(("Test.cs", Input));
        var first = GeneratorHarness.Run(compilation);
        var second = GeneratorHarness.Run(compilation);

        Assert.Equal(first.Text, second.Text);
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static void AssertNoDiagnostics(System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics)
    {
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error || d.Severity == DiagnosticSeverity.Warning);
    }
}
