using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotNetMapper.Generator.Tests;

internal static class GeneratorHarness
{
    public static CSharpParseOptions ParseOptions { get; } = CSharpParseOptions.Default
        .WithLanguageVersion(LanguageVersion.Latest)
        .WithFeatures([new KeyValuePair<string, string>("InterceptorsNamespaces", "DotNetMapper.Generated")]);

    public static ImmutableArray<MetadataReference> References { get; } = BuildReferences();

    public static CSharpCompilation CreateCompilation(params (string Name, string Source)[] sources)
    {
        SyntaxTree[] trees = sources
            .Select(static s => CSharpSyntaxTree.ParseText(s.Source, ParseOptions, path: s.Name))
            .ToArray();

        return CSharpCompilation.Create(
            "TestAssembly",
            trees,
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
    }

    public static GeneratorDriver CreateDriver() =>
        CSharpGeneratorDriver.Create(
            [new MapperGenerator().AsSourceGenerator()],
            parseOptions: ParseOptions,
            driverOptions: new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

    public static (string Text, ImmutableArray<Diagnostic> Diagnostics) Run(Compilation compilation)
    {
        GeneratorDriver driver = CreateDriver();
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out Compilation outputCompilation, out _);
        return (GetGeneratedText(driver), outputCompilation.GetDiagnostics());
    }

    public static string GetGeneratedText(GeneratorDriver driver) =>
        driver.GetRunResult().Results
            .SelectMany(static r => r.GeneratedSources)
            .Select(static s => s.SourceText.ToString())
            .FirstOrDefault() ?? "";

    private static ImmutableArray<MetadataReference> BuildReferences()
    {
        var references = new List<MetadataReference>();
        if (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") is string tpa)
        {
            foreach (string path in tpa.Split(Path.PathSeparator))
            {
                references.Add(MetadataReference.CreateFromFile(path));
            }
        }

        references.Add(MetadataReference.CreateFromFile(typeof(DotNetMapper.Mapper).Assembly.Location));
        return references.ToImmutableArray();
    }
}
