// See https://aka.ms/new-console-template for more information

using DynamicLinq;
using Expression_Evaulator;

Console.WriteLine("Hello, World!");

var expression = @"Count >= 125 && Name == ""Tv""";
        
var products = new List<Product>
{
    new Product {Name = "Speaker", Description = "Audio",Count = 125},
    new Product {Name = "Tv", Description = "Video",Count = 100},
    new Product {Name = "Book", Description = "Entertainment",Count = 200},
};
Console.WriteLine(products.Where(expression).Count());
Console.WriteLine(products.OrderBy("Count").First().Count);