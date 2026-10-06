using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotNetMapper.Generator;

[Generator(LanguageNames.CSharp)]
public sealed class MapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var models = context.SyntaxProvider
            .CreateSyntaxProvider(IsMapCallSyntax, GetMapCallModel)
            .WithTrackingName("MapCalls")
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .Collect();

        context.RegisterSourceOutput(models, static (ctx, models) =>
        {
            string text = SourceEmitter.Emit(models);
            if (text.Length > 0)
            {
                ctx.AddSource("DotNetMapper.Interceptors.g.cs", text);
            }
        });
    }

    private static bool IsMapCallSyntax(SyntaxNode node, CancellationToken _)
    {
        if (node is not InvocationExpressionSyntax invocation ||
            invocation.ArgumentList.Arguments.Count != 1)
        {
            return false;
        }

        return invocation.Expression switch
        {
            // `Map<A, B>(x)` after `using static DotNetMapper.Mapper;`.
            GenericNameSyntax { Identifier.ValueText: "Map", TypeArgumentList.Arguments.Count: 2 } => true,
            // `Mapper.Map<A, B>(x)`.
            MemberAccessExpressionSyntax { Name: GenericNameSyntax { Identifier.ValueText: "Map", TypeArgumentList.Arguments.Count: 2 } } => true,
            _ => false,
        };
    }

    private static MapCallModel? GetMapCallModel(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol method ||
            method.ContainingType.ToDisplayString() != "DotNetMapper.Mapper" ||
            method.ContainingAssembly?.Name != "DotNetMapper" ||
            method.Name != "Map" ||
            method.TypeArguments.Length != 2)
        {
            return null;
        }

        ITypeSymbol inputType = method.TypeArguments[0];
        ITypeSymbol outputType = method.TypeArguments[1];

        if (ContainsTypeParameter(inputType) || ContainsTypeParameter(outputType) ||
            inputType is IErrorTypeSymbol || outputType is IErrorTypeSymbol ||
            inputType.IsAnonymousType || outputType.IsAnonymousType ||
            (inputType as INamedTypeSymbol)?.IsFileLocal == true ||
            (outputType as INamedTypeSymbol)?.IsFileLocal == true)
        {
            return null;
        }

        Compilation compilation = semanticModel.Compilation;
        if (!compilation.IsSymbolAccessibleWithin(inputType, compilation.Assembly) ||
            !compilation.IsSymbolAccessibleWithin(outputType, compilation.Assembly))
        {
            return null;
        }

        if (IsInsideExpressionLambda(invocation, semanticModel, cancellationToken))
        {
            return null;
        }

        var location = Microsoft.CodeAnalysis.CSharp.CSharpExtensions.GetInterceptableLocation(semanticModel, invocation, cancellationToken);
        if (location is null)
        {
            return null;
        }

        return new MapCallModel(
            InputType: FullyQualified(inputType),
            OutputType: FullyQualified(outputType),
            InputIsReferenceOrNullable: IsReferenceOrNullable(inputType),
            Properties: MatchProperties(inputType, outputType),
            LocationVersion: location.Version,
            LocationData: location.Data,
            SortKey: $"{invocation.SyntaxTree.FilePath}:{invocation.SpanStart}");
    }

    private static bool ContainsTypeParameter(ITypeSymbol type)
    {
        return type switch
        {
            ITypeParameterSymbol => true,
            IArrayTypeSymbol array => ContainsTypeParameter(array.ElementType),
            IPointerTypeSymbol pointer => ContainsTypeParameter(pointer.PointedAtType),
            INamedTypeSymbol named => named.TypeArguments.Any(ContainsTypeParameter),
            _ => false,
        };
    }

    private static bool IsInsideExpressionLambda(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        foreach (SyntaxNode ancestor in invocation.Ancestors())
        {
            if (ancestor is not LambdaExpressionSyntax lambda)
            {
                continue;
            }

            if (semanticModel.GetTypeInfo(lambda, cancellationToken).ConvertedType is INamedTypeSymbol { OriginalDefinition: { } original } &&
                original.ToDisplayString() == "System.Linq.Expressions.Expression<TDelegate>")
            {
                return true;
            }
        }

        return false;
    }

    private static string FullyQualified(ITypeSymbol type) =>
        type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    private static bool IsReferenceOrNullable(ITypeSymbol type) =>
        type.IsReferenceType ||
        type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };

    private static EquatableArray<string> MatchProperties(ITypeSymbol inputType, ITypeSymbol outputType)
    {
        Dictionary<string, IPropertySymbol> sources = PublicProperties(inputType, requireGetter: true);
        Dictionary<string, IPropertySymbol> targets = PublicProperties(outputType, requireGetter: false);

        var names = new List<string>();
        foreach (KeyValuePair<string, IPropertySymbol> target in targets)
        {
            if (sources.TryGetValue(target.Key, out IPropertySymbol? source) &&
                SymbolEqualityComparer.Default.Equals(source.Type, target.Value.Type))
            {
                names.Add(target.Key);
            }
        }

        return new EquatableArray<string>(names.ToArray());
    }

    private static Dictionary<string, IPropertySymbol> PublicProperties(ITypeSymbol type, bool requireGetter)
    {
        var result = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);

        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (member is not IPropertySymbol property ||
                    property.IsStatic ||
                    property.IsIndexer ||
                    property.DeclaredAccessibility != Accessibility.Public)
                {
                    continue;
                }

                if (requireGetter)
                {
                    if (property.GetMethod is not { DeclaredAccessibility: Accessibility.Public })
                    {
                        continue;
                    }
                }
                else if (property.SetMethod is not { DeclaredAccessibility: Accessibility.Public })
                {
                    continue;
                }

                // Walking from most-derived to base, the first occurrence of a name wins.
                if (!result.ContainsKey(property.Name))
                {
                    result[property.Name] = property;
                }
            }
        }

        return result;
    }
}
