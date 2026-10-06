using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace DotNetMapper;

internal static class PropertyMatcher
{
    public static IEnumerable<(PropertyInfo Source, PropertyInfo Target)> Match(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type input,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type output)
    {
        var sources = MatchedProperties(input, source: true);
        var targets = MatchedProperties(output, source: false);

        var sourceByName = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            sourceByName[source.Name] = source;
        }

        foreach (var target in targets)
        {
            if (sourceByName.TryGetValue(target.Name, out var source) && source.PropertyType == target.PropertyType)
            {
                yield return (source, target);
            }
        }
    }

    private static List<PropertyInfo> MatchedProperties(
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] Type type,
        bool source)
    {
        var result = new Dictionary<string, PropertyInfo>(StringComparer.Ordinal);

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            if (source)
            {
                if (property.GetMethod is not { IsPublic: true })
                {
                    continue;
                }
            }
            else if (property.SetMethod is not { IsPublic: true })
            {
                continue;
            }

            if (!result.TryGetValue(property.Name, out var existing) || IsMoreDerived(property, existing))
            {
                result[property.Name] = property;
            }
        }

        return result.Values.ToList();
    }

    private static bool IsMoreDerived(PropertyInfo candidate, PropertyInfo existing)
    {
        int candidateDepth = BaseTypeDepth(candidate.DeclaringType);
        int existingDepth = BaseTypeDepth(existing.DeclaringType);
        return candidateDepth > existingDepth;
    }

    private static int BaseTypeDepth(Type? type)
    {
        int depth = 0;
        while (type is not null)
        {
            depth++;
            type = type.BaseType;
        }

        return depth;
    }
}
