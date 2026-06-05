using ConsoleAppLes3Lab;
using ConsoleAppLes3Lab.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(config.GetConnectionString("Local")).Options;
using var context = new DataContext(options);

//якщо БД пуста додаю об'єкти
if (!context.Products.Any())
{
    context.Products.AddRange(
        new Product
        {
            Name = "Хліб",
            Price = 35
        },
        new Product
        {
            Name = "Молоко",
            Price = 50
        },
        new Product
        {
            Name = "Цукор",
            Price = 45
        }
    );
    context.SaveChanges();//закінчую транзакцію
    Console.WriteLine("Db created!");
}
//метод додавання логів
void AddLog(string message)
{
    context.ActivLogs.Add(new ActivLog
    {
        Message = message
    });
    context.SaveChanges();
}
//AddLog("Програма запущена");

// метод показу всіх логів
void ShowLogs()
{
    var logs = context.ActivLogs.ToList();
    Console.WriteLine("\n=== LOGS ===");
    foreach (var log in logs)
    {
        Console.WriteLine(
            $"{log.Id}) {log.DateAt} - {log.Message}"
        );
    }
}

//метод додавання продукту через консоль
void AddProduct()
{
    Console.Write("Назва товару: ");
    string name = Console.ReadLine() ?? "";
    Console.Write("Ціна: ");
    decimal price = decimal.Parse(Console.ReadLine() ?? "0");
    context.Products.Add(new Product
    {
        Name = name,
        Price = price
    });
    context.SaveChanges();
    AddLog($"Додано товар: {name}");//відпрвляю повідомлення в ActivLog
    Console.WriteLine($" {name} Товар додано");
}
//метод видолення продукта за id
void DeleteProduct()
{
    Console.Write("Id товару: ");
    int id = int.Parse(Console.ReadLine() ?? "0");
    var product = context.Products.FirstOrDefault(x => x.Id == id);
    if (product == null)
    {
        Console.WriteLine("Товар не знайдено");
        return;
    }
    context.Products.Remove(product);
    context.SaveChanges();
    AddLog($"Видалено товар: {product.Name}");
    Console.WriteLine($" {product.Name} Товар видалено");
}
//метод показу всіх продактов
void ShowProducts()
{
    var products = context.Products.ToList();
    Console.WriteLine("\n=== PRODUCTS ===");
    foreach (var product in products)
    {
        Console.WriteLine($"{product.Id}) {product.Name} - {product.Price} грн");
    }
    AddLog("Перегляд списку товарів");
}
//switch для зручного тестування
while (true)
{
    Console.WriteLine("\n1 - Показати товари");
    Console.WriteLine("2 - Додати товар");
    Console.WriteLine("3 - Видалити товар");
    Console.WriteLine("4 - Показати логи");
    Console.WriteLine("0 - Вихід");

    string choice = Console.ReadLine() ?? "";

    switch (choice)
    {
        case "1": ShowProducts(); break;
        case "2": AddProduct(); break;
        case "3": DeleteProduct(); break;
        case "4": ShowLogs(); break;
        case "0": return;
    }
}