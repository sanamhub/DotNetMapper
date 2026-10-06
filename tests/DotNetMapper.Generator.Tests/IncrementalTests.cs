using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotNetMapper.Generator.Tests;

public sealed class IncrementalTests
{
    [Fact]
    public void Unrelated_tree_change_caches_every_step()
    {
        const string source = """
            using DotNetMapper;

            namespace Demo
            {
                public class Input { public int Id { get; set; } }
                public class Output { public int Id { get; set; } }

                public class Program
                {
                    public Output Go(Input x) => Mapper.Map<Input, Output>(x);
                }
            }
            """;

        GeneratorDriver driver = GeneratorHarness.CreateDriver();
        var compilation = GeneratorHarness.CreateCompilation(("Test.cs", source));

        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _, TestContext.Current.CancellationToken);
        string firstText = GeneratorHarness.GetGeneratedText(driver);

        Compilation updated = compilation.AddSyntaxTrees(
            CSharpSyntaxTree.ParseText("namespace Demo { public class Unrelated { } }", GeneratorHarness.ParseOptions, path: "Unrelated.cs", cancellationToken: TestContext.Current.CancellationToken));

        driver = driver.RunGeneratorsAndUpdateCompilation(updated, out _, out _, TestContext.Current.CancellationToken);
        string secondText = GeneratorHarness.GetGeneratedText(driver);

        Assert.Equal(firstText, secondText);
        Assert.NotEmpty(firstText);

        GeneratorRunResult result = driver.GetRunResult().Results.Single();
        foreach (IncrementalGeneratorRunStep step in result.TrackedSteps["MapCalls"])
        {
            Assert.True(
                step.Outputs.All(o => o.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged),
                $"step {step.Name} had a non-cached output");
        }
    }
}
