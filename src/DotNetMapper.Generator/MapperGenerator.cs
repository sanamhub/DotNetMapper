using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

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

        if (IsInsideExpressionTree(invocation, semanticModel, cancellationToken))
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

    private static bool IsInsideExpressionTree(InvocationExpressionSyntax invocation, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        // A lambda converted to Expression<T> and a query clause over IQueryable both compile to an
        // expression tree. Rewriting the call there would hand the query provider a call to a
        // file-local generated method it cannot translate. Query clauses have no lambda syntax,
        // so this walks the operation tree, where both show up as an anonymous function.
        bool insideLambda = false;
        for (IOperation? operation = semanticModel.GetOperation(invocation, cancellationToken); operation is not null; operation = operation.Parent)
        {
            if (operation is IAnonymousFunctionOperation)
            {
                insideLambda = true;
            }
            else if (insideLambda && IsExpressionOfT(operation.Type))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsExpressionOfT(ITypeSymbol? type) =>
        type is INamedTypeSymbol { IsGenericType: true } named &&
        named.OriginalDefinition.ToDisplayString() == "System.Linq.Expressions.Expression<TDelegate>";

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
        // Walking from most-derived to base, the first public property with a name wins, and only
        // then are its accessors checked. Checking first would fall back to a base property that
        // the generated code cannot bind to, because C# binds `x.Name` to the most-derived one.
        var mostDerived = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);
        for (ITypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            foreach (ISymbol member in current.GetMembers())
            {
                if (member is IPropertySymbol { IsStatic: false, IsIndexer: false, DeclaredAccessibility: Accessibility.Public } property &&
                    !mostDerived.ContainsKey(property.Name))
                {
                    mostDerived[property.Name] = property;
                }
            }
        }

        var result = new Dictionary<string, IPropertySymbol>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, IPropertySymbol> entry in mostDerived)
        {
            IMethodSymbol? accessor = requireGetter ? entry.Value.GetMethod : entry.Value.SetMethod;
            if (accessor is { DeclaredAccessibility: Accessibility.Public })
            {
                result[entry.Key] = entry.Value;
            }
        }

        return result;
    }
}
