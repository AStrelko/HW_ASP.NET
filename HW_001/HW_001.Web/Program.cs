// створення builder для налаштування застосунку
var builder = WebApplication.CreateBuilder(args);

// додаємо підтримку API-контролерів
builder.Services.AddControllers();
// додаємо підтримку OpenAPI 
builder.Services.AddOpenApi();
// створення готового web-застосунку
var app = builder.Build();

// увімкнення OpenAPI тільки в режимі розробки
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
// автоматичне перенаправлення HTTP -> HTTPS
app.UseHttpsRedirection();
// перевірка доступу користувача
app.UseAuthorization();
//логіка яка дозволяє додатку розуміти яка операція куди іде
app.MapControllers();
//запуск проекту
app.Run();