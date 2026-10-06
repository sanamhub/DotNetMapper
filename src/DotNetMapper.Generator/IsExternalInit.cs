namespace System.Runtime.CompilerServices;

// netstandard2.0 does not ship this compiler-required type. It only exists so `record` and
// `init` accessors compile for the generator itself; it is internal and ships nowhere.
internal static class IsExternalInit
{
}
