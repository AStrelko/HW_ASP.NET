
using Les04_pr.Con;
using Les04_pr.Con.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(config.GetConnectionString("Local")).Options;
using var context = new DataContext(options);


// додаю стартові значення
void SeedBooks()
{
    if (context.Books.Any())
    {
        Console.WriteLine("Дані вже існують");
        return;
    }

    var books = new List<Book>
    {
        new Book { Title = "Clean Code", Pages = 464, Price = 850 },
        new Book { Title = "The Pragmatic Programmer", Pages = 352, Price = 900 },
        new Book { Title = "C# in Depth", Pages = 900, Price = 1200 },
        new Book { Title = "Introduction to Algorithms", Pages = 1312, Price = 1500 },
        new Book { Title = "Design Patterns", Pages = 395, Price = 1100 },

        new Book { Title = "Refactoring", Pages = 431, Price = 980 },
        new Book { Title = "Head First C#", Pages = 800, Price = 700 },
        new Book { Title = "You Don't Know JS", Pages = 278, Price = 600 },
        new Book { Title = "JavaScript: The Good Parts", Pages = 176, Price = 500 },
        new Book { Title = "Eloquent JavaScript", Pages = 472, Price = 650 },

        new Book { Title = "Code Complete", Pages = 960, Price = 1300 },
        new Book { Title = "Domain-Driven Design", Pages = 560, Price = 1400 },
        new Book { Title = "Working Effectively with Legacy Code", Pages = 456, Price = 1000 },
        new Book { Title = "Clean Architecture", Pages = 432, Price = 950 },
        new Book { Title = "Algorithms Unlocked", Pages = 240, Price = 400 },

        new Book { Title = "Python Crash Course", Pages = 560, Price = 750 },
        new Book { Title = "Fluent Python", Pages = 770, Price = 1250 },
        new Book { Title = "Learning SQL", Pages = 320, Price = 550 },
        new Book { Title = "Grokking Algorithms", Pages = 256, Price = 500 },
        new Book { Title = "Pro ASP.NET Core", Pages = 850, Price = 1350 }
    };

    context.Books.AddRange(books);
    context.SaveChanges();

    Console.WriteLine("20 книг додано успішно!");
}

SeedBooks();

//додати книгу
void AddBook()
{
    while (true)
    {
        Console.Write("Ви бажаєте додати книгу (y/n): ");
        string qvery = Console.ReadLine() ?? "";
        if (qvery != "y") break;   
        Console.Write("Назва кнтги: ");
        string title = Console.ReadLine() ?? "";
        Console.Write("Ціна: ");
        decimal price = decimal.Parse(Console.ReadLine() ?? "0");
        try
        {
            var book = new Book
            {
                Title = title,
                Price = price,
            };
            context.Books.Add(book);
            Console.WriteLine($"{title} — кнтгу додано");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Помилка: " + ex.Message);
        }
    } 
    context.SaveChanges();
    Console.WriteLine();
}
Console.WriteLine("Кількість книг: " + context.Books.Count());
Console.WriteLine();

// оновлення книги
void UpdateBook()
{
    Console.Write("Введіть назву кнтги: ");
    string title = Console.ReadLine() ?? "";
    var book = context.Books.FirstOrDefault(b => b.Title == title);
    if (book == null)
    {
        Console.WriteLine("Книгу не знайдено");
        return;
    }
    Console.Write("Назва кнтги: ");
    book.Title = Console.ReadLine() ?? "";
    Console.Write("Ціна: ");
    book.Price = decimal.Parse(Console.ReadLine() ?? "0");
    context.SaveChanges();

    Console.WriteLine($"Книга оновлена");
    Console.WriteLine();
}

// видалення книги
void DeleteBook()
{
    Console.Write("Введіть назву кнтги: ");
    string title = Console.ReadLine() ?? "";
    var book = context.Books.FirstOrDefault(b => b.Title == title);
    if (book == null)
    {
        Console.WriteLine("Книгу не знайдено");
        return;
    }
    context.Books.Remove(book);
    context.SaveChanges();
    Console.WriteLine($"Книгу {title} видалено");
    Console.WriteLine();
}

// показати всі книги
void ShowBooks()
{
    var books = context.Books.ToList();
    foreach (var book in books)
    {
        Console.WriteLine($"{book.Title} - {book.Price}");
    }

    Console.WriteLine();
}

//отримати перші 10 кніжок в яких більше 50 сторінок
void prTask4()
{
    Console.WriteLine("############  task 4   ##########");
    var books = context.Books
        .AsNoTracking()
        .Where(b => b.Pages > 50)
        .Take(10)
        .ToList();
    foreach (var book in books)
    {
        Console.WriteLine($"{book.Title} - {book.Price}");
    }

    Console.WriteLine();
    Console.WriteLine("/-----------------------------------------/");
}

//отримати всі книжки : назва закінчуеться на "d",  < 100 сторінок, сортувати по назві
void prTask5()
{
    Console.WriteLine("##############   task 5    ######");
    var books = context.Books
        .AsNoTracking()
        .Where(b => b.Title.EndsWith("s"))
        .Where(b => b.Pages < 1500)
        .OrderBy(b => b.Title)
        .ToList();
    foreach (var book in books)
    {
        Console.WriteLine($"Id {book.Id}) {book.Title}- {book.Pages} - {book.Price}");
    }

    Console.WriteLine();
    Console.WriteLine("/-----------------------------------------/");
}
//отримати всі назви кніжок посиртовані по килькості сторінок без повторень
void prTask6()
{
    Console.WriteLine("############   task 6   ###############");
    var books = context.Books
        .AsNoTracking()
        .OrderBy(b => b.Pages)
        .Select(b => b.Title)
        .Distinct()
        .ToList();

    foreach (var book in books)
    {
        Console.WriteLine(book);
    }

    Console.WriteLine();
    Console.WriteLine("/-----------------------------------------/");
}

//тестування
//AddBook();
//ShowBooks();
//UpdateBook();
//ShowBooks();
//DeleteBook();
ShowBooks();

prTask4();
prTask5();
prTask6();