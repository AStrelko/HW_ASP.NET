using HW_003.Console;
using HW_003.Console.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(config.GetConnectionString("Local")).Options;
using var context = new DataContext(options);
using var transaction = context.Database.BeginTransaction();

//метод додавання логів
void AddLog(string message)
{
    context.Logs.Add(new Log
    {
        Message = message
    });
    context.SaveChanges();
}
// метод показу всіх логів
void ShowLogs()
{
    var logs = context.Logs.ToList();
    Console.WriteLine("\n=== LOGS ===");
    foreach (var log in logs)
    {
        Console.WriteLine(
            $"{log.Id}) {log.CreatedAt} - {log.Message}"
        );
    }
}
// 1) додати замовлення
void AddOrder()
{
    Console.Write("Назва товару: ");
    string title = Console.ReadLine() ?? "";
    Console.Write("Адреса: ");
    string street = Console.ReadLine() ?? "";
    Console.Write("Ціна: ");
    decimal price = decimal.Parse(Console.ReadLine() ?? "0");
    try
    {
        var order = new Order
        {
            Title = title,
            Date = DateTime.Now,
            StreetName = street,
            Price = price,
            CreatedAt = DateTime.Now
        };
        context.Orders.Add(order);
        context.BankTransactions.Add(new BankTransaction
        {
            Amount = price,
            Description = "Оплата за замовлення",
            CreatedAt = DateTime.Now
        });
        context.SaveChanges();
        AddLog($"Додано товар: {title}");
        Console.WriteLine($"{title} — товар додано");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Помилка: " + ex.Message);
    }
}
//оновити замовлення
void UpdateOrder()
{
    Console.Write("ID: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        return;
    var order = context.Orders.FirstOrDefault(x => x.Id == id);
    if (order == null)
    {
        Console.WriteLine("Не знайдено");
        return;
    }
    Console.Write("Нова назва: ");
    order.Title = Console.ReadLine() ?? order.Title;
    Console.Write("Нова адреса: ");
    order.StreetName = Console.ReadLine() ?? order.StreetName;
    Console.Write("Нова ціна: ");
    if (decimal.TryParse(Console.ReadLine(), out decimal price))
    {
        order.Price = price;
    }
    context.SaveChanges();
    AddLog($"Оновлено замовлення ID: {id}");
    Console.WriteLine($"Оновлено замовлення ID: {id}");
}
//видалити замовлення
void DeleteOrder()
{
    Console.Write("ID: ");
    if (!int.TryParse(Console.ReadLine(), out int id))
        return;
    var order = context.Orders.FirstOrDefault(x => x.Id == id);
    if (order == null)
    {
        Console.WriteLine("Не знайдено");
        return;
    }
    context.Orders.Remove(order);
    context.SaveChanges();
    AddLog($"Видалено замовлення ID: {id}");
    Console.WriteLine($"Видалено замовлення ID: {id}");
}
//показати всі замовлення
void ShowOrders()
{
    var orders = context.Orders.ToList();
    Console.WriteLine("\n=== ORDERS ===");
    foreach (var o in orders)
    {
        Console.WriteLine($"{o.Id}) {o.Title} | {o.Price} | {o.StreetName}");
    }
}
// 5) Отримати перші 5 замовлень
void ShowFirstFive()
{
    var orders = context.Orders.Take(5).ToList();
    foreach (var o in orders)
    {
        Console.WriteLine($"{o.Id}) {o.Title} | {o.Price}");
    }
}
// 6) Отримати замовлення з ціною більше ніж Х
void ShowMoreThanPrice()
{
    Console.Write("Ціна > ");
    if (!decimal.TryParse(Console.ReadLine(), out decimal price)) return;
    var orders = context.Orders.Where(x => x.Price > price).ToList();

    foreach (var o in orders)
    {
        Console.WriteLine($"{o.Id}) {o.Title} | {o.Price}");
    }
}
// 7) Отримати усі вулиці замовлень 
void ShowStreets()
{
    var streets = context.Orders.Select(x => x.StreetName).Distinct().ToList();
    foreach (var s in streets)
    {
        Console.WriteLine(s);
    }
}

while (true)
{
    Console.WriteLine("0 - Exit");
    Console.WriteLine("1 - Add");
    Console.WriteLine("2 - Update");
    Console.WriteLine("3 - Delete");
    Console.WriteLine("4 - Show All");
    Console.WriteLine("5 - First 5");
    Console.WriteLine("6 - Price > X");
    Console.WriteLine("7 - Streets");
    Console.WriteLine("8 - Logs");

    int choice = int.Parse(Console.ReadLine());

    switch (choice)
    {
        case 0: return;
        case 1: AddOrder(); break;
        case 2: UpdateOrder(); break;
        case 3: DeleteOrder(); break;
        case 4: ShowOrders(); break;
        case 5: ShowFirstFive(); break;
        case 6: ShowMoreThanPrice(); break;
        case 7: ShowStreets(); break;
        case 8: ShowLogs(); break;
    }
}