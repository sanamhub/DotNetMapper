using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DotNetMapper.Tests;

// A generic call site cannot be intercepted, so this always takes the runtime path.
internal static class RuntimePath
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static TOut Map<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TIn,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOut>(TIn input)
        where TOut : new()
        => Mapper.Map<TIn, TOut>(input);
}
