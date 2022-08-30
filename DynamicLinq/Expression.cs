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

    public static IEnumerable<TSource> Where<TSource>(this IEnumerable<TSource> source, string expression)
    {
        var props = source.First().GetType().GetProperties();
        var expressionProps = props.Where(x => expression.Contains(x.Name)).ToList();
        if (expressionProps.Count() > 0)
        {
            foreach (var item in expressionProps)
            {
                int index = expression.IndexOf(item.Name);
                //expression.Remove(index, item.Name.Length).Insert(index, "t." + item.Name);
                expression = expression.Replace(item.Name, "t." + item.Name);
            }
        }

        var options = ScriptOptions.Default.AddReferences(typeof(TSource).Assembly);
        var generatedExpression =
            CSharpScript.EvaluateAsync<Func<TSource, bool>>("t =>" + expression, options).Result;
        return source.Where(generatedExpression);
    }

    public static IEnumerable<TSource> OrderBy<TSource>(this IEnumerable<TSource> source, string expression)
    {
        var options = ScriptOptions.Default.AddReferences(typeof(TSource).Assembly);
        var generatedExpression =
            CSharpScript.EvaluateAsync<Func<TSource, object>>("t =>t." + expression, options).Result;
        return source.OrderBy(generatedExpression);
    }
}