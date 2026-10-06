using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace DotNetMapper;

internal static class MapperCache<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TInput,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOutput>
    where TOutput : new()
{
    // One delegate per closed generic type pair. The CLR runs the static initializer once,
    // thread-safely, so no lock and no dictionary are needed.
    internal static readonly Func<TInput, TOutput> Map = Build();

    private static Func<TInput, TOutput> Build()
    {
        var input = Expression.Parameter(typeof(TInput), "input");
        var bindings = new List<MemberBinding>();
        foreach (var (source, target) in PropertyMatcher.Match(typeof(TInput), typeof(TOutput)))
        {
            bindings.Add(Expression.Bind(target, Expression.Property(input, source)));
        }

        var body = Expression.MemberInit(Expression.New(typeof(TOutput)), bindings);
        return Expression.Lambda<Func<TInput, TOutput>>(body, input).Compile();
    }
}
