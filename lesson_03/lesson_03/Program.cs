//работа з БД
// NuGet пакети:
//Microsoft.EntityFrameworkCore - обгортка для зручной праці з БД
//Microsoft.EntityFrameworkCore.SqlServer - з якою БД буду працювати
//Microsoft.EntityFrameworkCore.Tools
//Microsoft.Extensions.Configuration.Json
//Microsoft.EntityFrameworkCore.Design
//створ DataContext.cs в якому роблю контекст: свор БД, роблю підключення


//отримую всі данні з "config.json" зберегаю в зміну config
//після зміни властивостей "config.json" (Build Action = Content, Copy to Output Directory = Copy always)
// В NuGet зробити Restore
//роблю міграцію
// в Terminal 
//dotnet clean (якщо до цого були помилки ВІДЧИЩУЮ)
//dotnet build (піся цого перезбераю)
//dotnet ef migrations add InitialMigrations (створ міграцію)
// dotnet ef database update (створ БД)

using System;
using System.Linq;
using lesson_03;
using lesson_03.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

var config = new ConfigurationBuilder().AddJsonFile("config.json").Build();
// передаю конфігурацію з зовні через config отримую підключення яке зозначено як Local
var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(config.GetConnectionString("Local")).Options;
//cтвор. підключення
using var context = new DataContext(options);

//context.Set<User>();//витягую таблицю User
//context.Add();
//context.Remove();
//context.Update();
//context.Users//отримати данні
/*
//додаю користувача
context.Users.Add(new User
{
    FirstName = "John",
    LastName = "Doe",
    BirthDate =  DateTime.Now.AddYears(-30)
});
*/
//додаю користувачів
// Добавляем данные только если таблица пустая
if (!context.Users.Any())
{
    context.Users.AddRange(
        new User
        {
            FirstName = "John",
            LastName = "Doe",
            BirthDate = new DateTime(1995, 5, 10)
        },
        new User
        {
            FirstName = "Jane",
            LastName = "Smith",
            BirthDate = new DateTime(2000, 8, 15)
        },
        new User
        {
            FirstName = "Peter",
            LastName = "Brown",
            BirthDate = new DateTime(1988, 12, 1)
        }
    );

    context.SaveChanges(); //завершую транзакцію
    Console.WriteLine("Db created!");
}

//отримую всіх користувачив
    var users = context.Users.ToList();//ченч трекер

    Console.WriteLine($"\nUsers count: {users.Count}\n");

    foreach (var user in users)
    {
        Console.WriteLine(
            $"{user.Id}. {user.FirstName} {user.LastName} | {user.BirthDate:d}"
        );
    }
    
    
    //     Lesson 4     //
    //змінюю і'мя всіх користувачів
    foreach (var user in users)
    {
        user.FirstName = "Test";
    }
    context.SaveChanges();
    
    // додаю кучу :) данних
    string[] names = ["Andriy","Ivan","Rob","Den"];
    for(var i = 0; i <= 1000;i++)
    {
        context.Users.Add(new User
        {
            FirstName = names[Random.Shared.Next(names.Length)], //рондомно оберає з масиву імен
            LastName = "Dou",
            BirthDate = DateTime.Now.AddYears(Random.Shared.Next(80)) //рандомна дата нар < 80
        });
    }
    context.SaveChanges();

    // users.Take(20) - виведе перші 20
    foreach (var user in users.Take(20))
    {
        Console.WriteLine(user);
    }
    // або працюю зразу з першими 20
     var usersTop20 = context.Users.AsNoTracking().Take(20).ToList();
     
     // щоб не витягувати всю інформацію, а тільки окремі колонки
     var usersOnliName = context.Users
         .AsNoTracking()// 
        //спочатку роблю фільтрацію
         .Where(u => u.FirstName.StartsWith("A"))//починаються з "A"
         .Where(u => u.FirstName.EndsWith("a"))// занінчується
         .Where(u => u.FirstName.Contains("an"))// містить в собі
         .Where(u => u.FirstName.ToLower().Contains("an"))//ToLower() якщо требо враховувати реестр
         // EF.Functions. ... - вбудовані функціі
         .Where(u => EF.Functions.Like(u.FirstName, "%an%")) //true якщо і'мя содерже "an"
         .OrderBy(u => u.FirstName)// після фільтрів проводим сортування
         .Select(u => new//тільки окремі колонки
         {
             u.FirstName,
             u.LastName
         })
         .Distinct()// без повторень
         .Take(20)//перші 20
         .Skip(5)//пропустити перші 5
         .ToList();// і тільки ToList() відправляє запит

     foreach (var user in usersOnliName)
     {
         Console.WriteLine(user.FirstName + " " + user.LastName);
     }
     
     //отримую час виконнаня коду 
     // потрібно           using System.Diagnostics;
     var sw = Stopwatch.StartNew();
     Console.WriteLine($"Time: {sw.ElapsedMilliseconds} ms");