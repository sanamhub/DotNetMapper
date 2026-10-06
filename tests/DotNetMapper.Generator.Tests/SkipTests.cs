using Microsoft.CodeAnalysis;

namespace DotNetMapper.Generator.Tests;

public sealed class SkipTests
{
    [Fact]
    public void Open_generic_call_site_is_not_intercepted()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class Program
                {
                    public TOut Go<TIn, TOut>(TIn x) where TOut : new() => Mapper.Map<TIn, TOut>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal("", text);
        AssertNoErrors(diagnostics);
    }

    [Fact]
    public void Private_nested_types_are_not_intercepted()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class Outer
                {
                    private class Input { public int Id { get; set; } }
                    private class Output { public int Id { get; set; } }

                    private object Go(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal("", text);
        AssertNoErrors(diagnostics);
    }

    [Fact]
    public void File_local_types_are_not_intercepted()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                file class Input { public int Id { get; set; } }
                file class Output { public int Id { get; set; } }

                file class Program
                {
                    public Output Go(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal("", text);
        AssertNoErrors(diagnostics);
    }

    [Fact]
    public void Type_parameter_inside_generic_class_is_not_intercepted()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class Output { public int Id { get; set; } }

                public class Program<T>
                {
                    public Output Go(T x) => Mapper.Map<T, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal("", text);
        AssertNoErrors(diagnostics);
    }

    [Fact]
    public void Expression_tree_lambda_is_not_intercepted()
    {
        const string source = """
            using System;
            using System.Linq.Expressions;
            using DotNetMapper;

            namespace Demo
            {
                public class Input { public int Id { get; set; } }
                public class Output { public int Id { get; set; } }

                public class Program
                {
                    public Expression<Func<Input, Output>> Go() => x => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal("", text);
        AssertNoErrors(diagnostics);
    }

    [Theory]
    [InlineData("from x in source select Mapper.Map<Input, Output>(x)")]
    [InlineData("from x in source where Mapper.Map<Input, Output>(x).Id > 0 select x")]
    [InlineData("from x in source orderby Mapper.Map<Input, Output>(x).Id select x")]
    [InlineData("from x in source let y = Mapper.Map<Input, Output>(x) select y")]
    [InlineData("from x in source group Mapper.Map<Input, Output>(x) by x.Id")]
    public void Query_clause_over_IQueryable_is_not_intercepted(string query)
    {
        string source = $$"""
            using System.Linq;
            using DotNetMapper;

            namespace Demo
            {
                public class Input { public int Id { get; set; } }
                public class Output { public int Id { get; set; } }

                public class Program
                {
                    public object Go(IQueryable<Input> source) => {{query}};
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal("", text);
        AssertNoErrors(diagnostics);
    }

    [Fact]
    public void Method_group_is_not_intercepted()
    {
        const string source = """
            using System;
            using DotNetMapper;

            namespace Demo
            {
                public class Input { public int Id { get; set; } }
                public class Output { public int Id { get; set; } }

                public class Program
                {
                    public Func<Input, Output> Go() => Mapper.Map<Input, Output>;
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal("", text);
        AssertNoErrors(diagnostics);
    }

    [Fact]
    public void Another_mapper_class_is_not_intercepted()
    {
        const string source = """
            namespace Other
            {
                public static class Mapper
                {
                    public static TOut Map<TIn, TOut>(TIn x) where TOut : new() => new TOut();
                }

                public class Input { public int Id { get; set; } }
                public class Output { public int Id { get; set; } }

                public class Program
                {
                    public Output Go(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        var (text, diagnostics) = GeneratorHarness.Run(GeneratorHarness.CreateCompilation(("Test.cs", source)));

        Assert.Equal("", text);
        AssertNoErrors(diagnostics);
    }

    private static void AssertNoErrors(System.Collections.Immutable.ImmutableArray<Diagnostic> diagnostics)
    {
        Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
    }
}
