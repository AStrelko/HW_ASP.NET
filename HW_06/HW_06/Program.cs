using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using HW_06;
using HW_06.Filters;
using HW_06.Helpers;
using HW_06.Middleware;
using HW_06.Models;
using HW_06.Profile;
using HW_06.Services;
using HW_06.Services.Interfaces;
using HW_06.Validators.MeetingValid;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

//
// Контролери
//

builder.Services.AddControllers();

//
// База даних
//

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"));
});

//
// ASP.NET Core Identity
//

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

//
// AutoMapper
//

// Реєструє всі профілі AutoMapper зі збірки,
// у якій знаходиться MeetingMappingProfile.
builder.Services.AddAutoMapper(
    typeof(MeetingMappingProfile));

//
// Сервіси
//

builder.Services.AddScoped<
    IMeetingService,
    MeetingService>();

builder.Services.AddScoped<
    IParticipantService,
    ParticipantService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddSingleton<
    IFileStorageService,
    LocalFileStorageService>();

//
// FluentValidation
//

// Реєструє всі FluentValidation-валідатори
// зі збірки застосунку.
builder.Services.AddValidatorsFromAssemblyContaining<
    MeetingCreateValidator>();

// Реєструє універсальний фільтр валідації DTO.
builder.Services.AddScoped(
    typeof(ValidationFilter<>));

//
// Версіонування API
//

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion =
            new ApiVersion(2, 0);

        options.AssumeDefaultVersionWhenUnspecified =
            true;

        options.ReportApiVersions =
            true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat =
            "'v'VVV";

        options.SubstituteApiVersionInUrl =
            true;
    });

//
// Публічні файли зустрічей
//

builder.Services.AddScoped<
    IAttachmentService,
    AttachmentService>();

//
// Приватні файли учасників
//

builder.Services.AddScoped<
    IPrivateAttachmentService,
    PrivateAttachmentService>();

//
// Swagger
//

builder.Services.ConfigureOptions<
    ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile =
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath =
        Path.Combine(
            AppContext.BaseDirectory,
            xmlFile);

    options.IncludeXmlComments(
        xmlPath);
});

var app = builder.Build();

//
// Початкове наповнення бази даних
//

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<DataContext>();

    var userManager =
        scope.ServiceProvider
            .GetRequiredService<
                UserManager<ApplicationUser>>();

    await SeedData.InitializeAsync(
        context,
        userManager);
}

//
// Middleware глобальної обробки винятків
//

app.UseMiddleware<
    ExceptionHandlingMiddleware>();

//
// Middleware технічного обслуговування
//

app.UseMiddleware<
    MaintenanceMiddleware>();

//
// Swagger
//

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        var provider =
            app.Services
                .GetRequiredService<
                    IApiVersionDescriptionProvider>();

        foreach (var description
                 in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

//
// HTTPS
//

app.UseHttpsRedirection();

//
// Identity
//

app.UseAuthentication();
app.UseAuthorization();

//
// Статичні публічні файли
//

var publicFilesPath =
    Path.Combine(
        app.Environment.ContentRootPath,
        "uploads",
        "PublicFile");

// Створює каталог публічних файлів,
// якщо він ще не існує.
Directory.CreateDirectory(
    publicFilesPath);

// Надає доступ до публічних файлів
// через URL-адресу /uploads.
app.UseStaticFiles(
    new StaticFileOptions
    {
        FileProvider =
            new PhysicalFileProvider(
                publicFilesPath),

        RequestPath =
            "/uploads"
    });

//
// Контролери
//

app.MapControllers();

app.Run();