namespace DotNetMapper.Generator;

// Only strings, ints, bools and EquatableArray<string>. Never ISymbol, SyntaxNode, Location or
// SemanticModel: incremental caching compares these by value.
internal sealed record MapCallModel(
    string InputType,
    string OutputType,
    bool InputIsReferenceOrNullable,
    EquatableArray<string> Properties,
    int LocationVersion,
    string LocationData,
    string SortKey);
