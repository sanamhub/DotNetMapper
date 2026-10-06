using System.Diagnostics.CodeAnalysis;

namespace DotNetMapper;

/// <summary>
/// Maps objects of one type to a new object of another type by copying public properties
/// that have the same name and the same type.
/// </summary>
public static class Mapper
{
    /// <summary>
    /// Creates a new <typeparamref name="TOutput"/> and copies into it every public readable property
    /// of <paramref name="inputObject"/> that has a public settable (or init) property on
    /// <typeparamref name="TOutput"/> with the same name and the same type.
    /// </summary>
    /// <remarks>
    /// Calls with concrete types are replaced at compile time by generated code (no reflection).
    /// Other calls use a delegate compiled once per type pair.
    /// </remarks>
    /// <typeparam name="TInput">The source type.</typeparam>
    /// <typeparam name="TOutput">The destination type. Must have a public parameterless constructor.</typeparam>
    /// <param name="inputObject">The object to copy from.</param>
    /// <returns>A new <typeparamref name="TOutput"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="inputObject"/> is <see langword="null"/>.</exception>
    public static TOutput Map<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TInput,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] TOutput>(
        TInput inputObject)
        where TOutput : new()
    {
        // `is null` on an unconstrained generic compiles away for value types and never boxes.
        // ArgumentNullException.ThrowIfNull(object) would box a struct TInput on every call.
        if (inputObject is null)
        {
            ThrowArgumentNull(nameof(inputObject));
        }

        return MapperCache<TInput, TOutput>.Map(inputObject);
    }

    [DoesNotReturn]
    private static void ThrowArgumentNull(string paramName) => throw new ArgumentNullException(paramName);
}
