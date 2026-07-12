using less_06.App.Services;
using less_06.App.Storage;
using Microsoft.EntityFrameworkCore;
/*
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// опис залежнстей з низу до верху
builder.Services.AddDbContext<DataContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ProductServices>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
*/

var db = new SQLDatabase();
    
//створю інтерфейс який містить базові операції по работі з BD
interface IDatabase
{
    string Insert(string data);
}

void PrintCommand(SQLDatabase database)
{
    Console.WriteLine(database.Insert("123"));
}

class SQLDatabase: IDatabase
{
    public string Insert(string data)
    {
        return $"insert table values ('{data}')";
    }
}

 class RedisDatabase: IDatabase
 {
     public string Insert(string data)
     {
         return $"Redis inserted ('{data}')";
     }
 }