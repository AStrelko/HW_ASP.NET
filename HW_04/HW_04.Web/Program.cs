using HW_04.Storege;
using HW_04.Core.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("config.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
//якщо БД пуста задою стартові значення
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!context.Authors.Any() && !context.Movies.Any())
    {
        var authors = new List<Author>
        {
            new Author { AuthorFirstName = "Steven", AuthorLastName = "Spielberg" },
            new Author { AuthorFirstName = "Christopher", AuthorLastName = "Nolan" },
            new Author { AuthorFirstName = "Quentin", AuthorLastName = "Tarantino" },
            new Author { AuthorFirstName = "James", AuthorLastName = "Cameron" },
            new Author { AuthorFirstName = "Ridley", AuthorLastName = "Scott" }
        };

        context.Authors.AddRange(authors);
        context.SaveChanges();

        var movies = new List<Movie>
        {
            new Movie { Title = "Inception", Year = 2010, Rating = 9, AuthorId = authors[1].Id },
            new Movie { Title = "Interstellar", Year = 2014, Rating = 10, AuthorId = authors[1].Id },
            new Movie { Title = "Django Unchained", Year = 2012, Rating = 9, AuthorId = authors[2].Id },
            new Movie { Title = "Pulp Fiction", Year = 1994, Rating = 10, AuthorId = authors[2].Id },
            new Movie { Title = "Avatar", Year = 2009, Rating = 8, AuthorId = authors[3].Id },
            new Movie { Title = "Titanic", Year = 1997, Rating = 9, AuthorId = authors[3].Id },
            new Movie { Title = "Gladiator", Year = 2000, Rating = 9, AuthorId = authors[4].Id },
            new Movie { Title = "Alien", Year = 1979, Rating = 8, AuthorId = authors[4].Id },
            new Movie { Title = "Jaws", Year = 1975, Rating = 8, AuthorId = authors[0].Id },
            new Movie { Title = "Catch Me If You Can", Year = 2002, Rating = 8, AuthorId = authors[0].Id }
        };

        context.Movies.AddRange(movies);
        context.SaveChanges();
    }
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    ShowAllMovies(context);
    ShowTaskHW(context);
}
//показати інформацію о всіх філльмах
static void ShowAllMovies(AppDbContext context)
{
    var movies = context.Movies
        .Include(m => m.Author)
        .ToList();

    foreach (var m in movies)
    {
        Console.WriteLine(
            $"Id: {m.Id} | " +
            $"Title: {m.Title} | " +
            $"Year: {m.Year} | " +
            $"Rating: {m.Rating} | " +
            $"Author: {m.Author.AuthorFirstName} {m.Author.AuthorLastName}"
        );
    }
}
//в функціі ввожу в консолі пошукову інформацію ( і'мя чи фамілію автора, чи назву , чи рік
// чи їх частину. Отримую фільтрований, перші 5, сортованих за рейтінгом масив.
// Та вивожу їх в консолі 
static void ShowTaskHW(AppDbContext context)
{
    Console.Write("Введіть пошуковий запит: ");
    string query = Console.ReadLine()?.ToLower() ?? "";

    if (string.IsNullOrWhiteSpace(query))
        return;

    var movies = context.Movies
        .AsNoTracking()//просто считую данні не відслідковую їх
        .Include(m => m.Author)//ромлю связь між таблицями
        .Where(m =>//пошук 
            m.Title.ToLower().Contains(query) ||// в назві 
            m.Author.AuthorFirstName.ToLower().Contains(query) ||// в і'мені 
            m.Author.AuthorLastName.ToLower().Contains(query) ||// в призвищі
            m.Year.ToString().Contains(query)// в році
        )
        .Distinct()// без повторень
        .OrderByDescending(m => m.Rating)//сортую по рейтенгу
        .Take(5)// перші 5
        .Select(m => new// виводжу те що хочу і як хочу
        {
            m.Title,
            m.Year,
            m.Rating,
            Author = m.Author.AuthorFirstName + " " + m.Author.AuthorLastName
        })
        .ToList();//відправляю запрос

    Console.WriteLine("\nРезультати пошуку:");
    if (movies.Count == 0)
    {
        Console.WriteLine("Нічого не знайдено");
        return;
    }
    foreach (var m in movies)// вивід
    {
        Console.WriteLine($"{m.Title} | {m.Year} | {m.Rating} | {m.Author}");
    }
}

app.Run();