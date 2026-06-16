using less_005Pr.Async;
using less_005Pr.Async.Models;
using Microsoft.EntityFrameworkCore;

using var context = new AppDbContext();

// створення БД (1 раз)
context.Database.EnsureCreated();

// Запуск фонової задачи
_ = Task.Run(ShowLastTransactionsPeriodicallyAsync);

// МЕНЮ
while (true)
{
    Console.WriteLine("\n1 - Додати транзакцію");
    Console.WriteLine("2 - Показати всі транзакції");
    Console.WriteLine("0 - Вихід");

    string choice = Console.ReadLine() ?? "";

    switch (choice)
    {
        case "1": await AddTransactionAsync(); break;
        case "2": await ShowAllTransactionsAsync(); break;
        case "0": return;
    }
}

// Показ всіх транзакцій
static async Task ShowAllTransactionsAsync()
{
    using var context = new AppDbContext();

    var transactions = await context.Transactions
        .AsNoTracking()
        .OrderByDescending(t => t.Date)
        .ToListAsync();

    Console.WriteLine("\nСписок транзакцій:");

    foreach (var t in transactions)
    {
        Console.WriteLine($"{t.Id} | {t.Date:g} | {t.Description} | {t.Amount}");
    }
}

// Додавання транзакціі
static async Task AddTransactionAsync()
{
    using var context = new AppDbContext();

    Console.Write("Назва: ");
    string description = Console.ReadLine() ?? "";

    Console.Write("Сума: ");
    decimal amount = decimal.Parse(Console.ReadLine() ?? "0");

    var transaction = new Transaction
    {
        Date = DateTime.Now,
        Description = description,
        Amount = amount
    };

    context.Transactions.Add(transaction);
    await context.SaveChangesAsync();

    // запись в файл
    string line = $"{transaction.Date:g} | {transaction.Description} | {transaction.Amount}";
    await File.AppendAllTextAsync("transactions.txt", line + Environment.NewLine);

    Console.WriteLine("Транзакцію додано.");
}

// отримання останіх 3
static async Task<List<Transaction>> GetLastThreeTransactionsAsync()
{
    using var context = new AppDbContext();

    return await context.Transactions
        .AsNoTracking()
        .OrderByDescending(t => t.Date)
        .Take(3)
        .ToListAsync();
}

// Фоновий вивід кожні 10 секунд (якось це не зручно в консольном проекті)
static async Task ShowLastTransactionsPeriodicallyAsync()
{
    while (true)
    {
        var transactions = await GetLastThreeTransactionsAsync();
        Console.WriteLine("\n--- Останні 3 транзакції ---");
        foreach (var t in transactions)
        {
            Console.WriteLine($"{t.Date:g} | {t.Description} | {t.Amount}");
        }
        await Task.Delay(10000);
    }
}

