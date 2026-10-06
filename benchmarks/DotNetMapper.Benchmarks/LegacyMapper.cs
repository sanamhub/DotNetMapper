using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace DotNetMapper.Benchmarks;

// The exact 1.0.2 implementation, renamed. It compiles the expression tree on every call
// because the value overload of GetOrAdd runs the factory unconditionally.
internal static class LegacyMapper
{
    private static readonly ConcurrentDictionary<(Type, Type), Func<object, object>> _mapFunctionCache = new();

    public static TOutput Map<TInput, TOutput>(TInput inputObject) where TOutput : new()
    {
        if (inputObject is null) throw new ArgumentNullException(nameof(inputObject));

        var mapFunction = _mapFunctionCache.GetOrAdd((typeof(TInput), typeof(TOutput)), CreateMapFunction<TInput, TOutput>());

        return (TOutput)mapFunction(inputObject);
    }

    #region Privates

    private static Func<object, object> CreateMapFunction<TInput, TOutput>()
    {
        var inputParameter = Expression.Parameter(typeof(object), "input");
        var inputVariable = Expression.Variable(typeof(TInput), "inputVariable");
        var outputVariable = Expression.Variable(typeof(TOutput), "outputVariable");

        var inputConvert = Expression.Convert(inputParameter, typeof(TInput));

        var inputAssign = Expression.Assign(inputVariable, inputConvert);

        var inputProperties = typeof(TInput).GetProperties();
        var outputProperties = typeof(TOutput).GetProperties();

        var expressions = new List<Expression>
        {
            Expression.Assign(outputVariable, Expression.New(typeof(TOutput)))
        };

        foreach (var inputProperty in inputProperties)
        {
            var outputProperty = outputProperties.FirstOrDefault(p => p.Name == inputProperty.Name && p.PropertyType == inputProperty.PropertyType);

            if (outputProperty is not null && outputProperty.CanWrite)
            {
                var inputPropertyValue = Expression.Property(inputVariable, inputProperty);

                var outputPropertyValue = Expression.Property(outputVariable, outputProperty);
                var setOutputPropertyValue = Expression.Assign(outputPropertyValue, inputPropertyValue);

                expressions.Add(setOutputPropertyValue);
            }
        }

        expressions.Add(outputVariable);

        var lambda = Expression.Lambda<Func<object, object>>(
            Expression.Block([inputVariable, outputVariable], new Expression[] { inputAssign }.Concat(expressions)),
            inputParameter
        );

        return lambda.Compile();
    }

    #endregion
}
