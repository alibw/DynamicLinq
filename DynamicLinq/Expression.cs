using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace DynamicLinq;

public static class Expression
{
    // public static async Task<Func<T, bool>> generateExpression<T>(this string expression)
    // {
    //     var options = ScriptOptions.Default.AddReferences(typeof(T).Assembly);
    //     var filterExpression = await CSharpScript.EvaluateAsync<Func<T, bool>>("t => t." + expression, options);
    //     return filterExpression;
    // }
    
    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source,string expression)
    {
        var options = ScriptOptions.Default.AddReferences(typeof(TSource).Assembly);
        var generatedExpression = CSharpScript.EvaluateAsync<Func<TSource, bool>>("t => t." + expression, options).Result;
        return source.Where(generatedExpression);
    }
}