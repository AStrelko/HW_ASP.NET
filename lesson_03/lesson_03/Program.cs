//работа з БД
// NuGet пакети:
//Microsoft.EntityFrameworkCore - обгортка для зручной праці з БД
//Microsoft.EntityFrameworkCore.SqlServer - з якою БД буду працювати
//Microsoft.EntityFrameworkCore.Tools
//Microsoft.Extensions.Configuration.Json
//Microsoft.EntityFrameworkCore.Design
//створ DataContext.cs в якому роблю контекст: свор БД, роблю підключення


//отримую всі данні з "config.json" зберегаю в зміну config

using System;
using System.Linq;
using lesson_03;
using lesson_03.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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
    var users = context.Users.ToList();

    Console.WriteLine($"\nUsers count: {users.Count}\n");

    foreach (var user in users)
    {
        Console.WriteLine(
            $"{user.Id}. {user.FirstName} {user.LastName} | {user.BirthDate:d}"
        );
    }