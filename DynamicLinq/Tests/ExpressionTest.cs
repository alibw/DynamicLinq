using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using Expression_Evaulator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace DynamicLinq.Tests;

[TestFixture]
public class ExpressionTest
{
    private HashSet<PortableExecutableReference> References { get; } = new HashSet<PortableExecutableReference> ();

    [Test]
    public void myTest()
    {
        var expression = @"Count >= 125 && Name == ""Tv""";
        
        var products = new List<Product>
        {
            new Product {Name = "Speaker", Description = "Audio",Count = 125},
            new Product {Name = "Tv", Description = "Video",Count = 100},
            new Product {Name = "Book", Description = "Entertainment",Count = 200},
        };
        Console.WriteLine(products.Where(expression).Count());
        Console.WriteLine(products.OrderBy("Count").First().Count);
    }

    [Test]
    public void memoryLeakTest()
    {
        var ex = Evaluate<Expression<Func<Product, bool>>>("x=>x.Count > 100");
        Console.WriteLine(ex);
    }

    public bool AddAssembly(string assemblyDll)
    {
        if (string.IsNullOrEmpty(assemblyDll)) return false;

        var file = Path.GetFullPath(assemblyDll);

        if (!File.Exists(file))
        {
            // check framework or dedicated runtime app folder
            var path = Path.GetDirectoryName(typeof(object).Assembly.Location);
            file = Path.Combine(path, assemblyDll);
            if (!File.Exists(file))
                return false;
        }

        if (References.Any(r => r.FilePath == file)) return true;

        try
        {
            var reference = MetadataReference.CreateFromFile(file);
            References.Add(reference);
        }
        catch
        {
            return false;
        }

        return true;
    }

    
    public void AddAssemblies(params string[] assembiles)
    {
        foreach (var item in assembiles)
        {
            AddAssembly(item);
        }
    }
    
    
    public T Evaluate<T>(string lambda)
    {
        var productReference = MetadataReference.CreateFromFile(typeof(Product).Assembly.Location);
        References.Add(productReference);
        AddAssemblies(
            "System.Private.CoreLib.dll",
            "System.Runtime.dll",
            "System.Console.dll",
            "System.Linq.dll",
            "System.Linq.Expressions.dll", // IMPORTANT!
            "System.Text.RegularExpressions.dll", // IMPORTANT!
            "System.IO.dll",
            "System.Net.Primitives.dll",
            "System.Net.Http.dll",
            "System.Private.Uri.dll",
            "System.Reflection.dll",
            "System.ComponentModel.Primitives.dll",
            "System.Collections.Concurrent.dll",
            "System.Collections.NonGeneric.dll",
            "Microsoft.CSharp.dll",
            "Microsoft.CodeAnalysis.dll",
            "Microsoft.CodeAnalysis.CSharp.dll"
        );
        var outerClass = 
                         @"using System.Linq.Expressions;
                           using System.Reflection;" + $"public static class Wrapper {{ public static Expression<Func<Product,bool>> expr = {lambda}; }}";
 
        var compilation = CSharpCompilation.Create("__myAssembly" + Guid.NewGuid(),
            new[] { CSharpSyntaxTree.ParseText(outerClass) },
            References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var assemblyLoadContext = new CollectibleAssemblyContext();
        using var ms = new MemoryStream();
 
        var cr = compilation.Emit(ms);
        
        if (!cr.Success)
        {
            throw new InvalidOperationException("Error in expression: " + cr.Diagnostics.First(e =>
                e.Severity == DiagnosticSeverity.Error).GetMessage());
        }
        
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = assemblyLoadContext.LoadFromStream(ms);
 
        var outerClassType = assembly.GetType("Wrapper");
 
        var exprField = outerClassType.GetField("expr", BindingFlags.Public | BindingFlags.Static);
        // ReSharper disable once PossibleNullReferenceException
        return (T)exprField.GetValue(null);
    }
}