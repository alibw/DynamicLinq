using Expression_Evaulator;
using NUnit.Framework;

namespace DynamicLinq.Tests;

[TestFixture]
public class ExpressionTest
{
    [Test]
    public void myTest()
    {
        var expression = "Count >= 125";
        
        var products = new List<Product>
        {
            new Product {Name = "Speaker", Description = "Audio",Count = 125},
            new Product {Name = "Tv", Description = "Video",Count = 100},
            new Product {Name = "Book", Description = "Entertainment",Count = 200},
        };
        Console.WriteLine(products.Where(expression).Count());
    }
}