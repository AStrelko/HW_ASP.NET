using lesson_03.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace lesson_03;

public class DataContext: DbContext
{
    /*
    public DataContext()
    {
        Database.EnsureCreated();//конструктор диветься чи є БД якщо ні створює
        // - не зможу оновити, тільки як що видалю стару БД та створю нову
    }
    */
    //требо використовувати міграціі
    /*
    // не правельне підключення
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=lesson_03;Trusted_Connection=True;");
    }
    */
    
    //для правелного підключення в проекті створ. конфиг. файл наприклад json  (config.json) де зберегаю
    // строку підключення та інші секретні данні
    
    public DataContext()
    {
    }
    //зєднаня з БД через конструктор в який передаю з зовні конфігурацію в Program.cs
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    // налаштування як буде створюваться таблиці в БД
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    modelBuilder.Entity<User>().Property(x => x.FirstName).HasMaxLength(50);
    //    modelBuilder.Entity<User>().Property(x => x.LastName).HasMaxLength(50);
    //}
    //метод який створує контекст який показує як підключитись до БД
    //потрібен для створеня міграціі
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();

            optionsBuilder.UseSqlServer(
                config.GetConnectionString("Local")
            );
        }
    }
    
    public DbSet<User> Users { get; set; }// Alt+Enter для зєднання з класом
    
}