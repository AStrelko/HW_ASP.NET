using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using HW_06;
using HW_06.DTOs.MeetingDTO;
using HW_06.DTOs.ParticipantDTO;
using HW_06.Helpers;
using HW_06.Profile;
using HW_06.Services;
using HW_06.Services.Interfaces;
using HW_06.Validators;
using HW_06.Validators.FileValid;
using HW_06.Validators.MeetingValid;
using HW_06.Validators.ParticipantValid;
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//
// AutoMapper
//

// Реєструє всі профілі AutoMapper зі збірки, у якій знаходиться MeetingMappingProfile.
builder.Services.AddAutoMapper(typeof(MeetingMappingProfile));

//
// Сервіси
//

builder.Services.AddScoped<IMeetingService, MeetingService>();

builder.Services.AddScoped<IParticipantService, ParticipantService>();

builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

//
// Валідатори зустрічей
//

builder.Services.AddScoped<IValidator<MeetingCreateDTO>, MeetingCreateValidator>();

builder.Services.AddScoped<IValidator<MeetingUpdateDTO>,MeetingUpdateValidator>();

builder.Services.AddScoped<IValidator<MeetingPartialUpdateDTO>, MeetingPartialUpdateValidator>();

//
// Валідатори учасників
//

builder.Services.AddScoped<IValidator<ParticipantCreateDTO>, ParticipantCreateValidator>();

builder.Services.AddScoped<IValidator<ParticipantUpdateDTO>, ParticipantUpdateValidator>();

builder.Services.AddScoped<IValidator<ParticipantPartialUpdateDTO>, ParticipantPartialUpdateValidator>();

//
// Версіонування API
//

builder.Services.AddApiVersioning(options => {
        options.DefaultApiVersion = new ApiVersion(2, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";

        options.SubstituteApiVersionInUrl = true;
    });

//
// Публічні файли зустрічей
//

builder.Services.AddScoped<IAttachmentService, AttachmentService>();

//
// Приватні файли учасників
//

builder.Services.AddScoped<PrivateDocumentValidator>();

builder.Services.AddScoped<IPrivateAttachmentService, PrivateAttachmentService>();

//
// Swagger
//

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

//
// Початкове наповнення бази даних
//

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    SeedData.Initialize(context);
}

//
// Конвеєр обробки HTTP-запитів
//

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

//
// Статичні публічні файли
//

var publicFilesPath = Path.Combine(app.Environment.ContentRootPath, "uploads", "PublicFile");

// Створює каталог публічних файлів, якщо він ще не існує.
Directory.CreateDirectory(publicFilesPath);

// Надає доступ до публічних файлів через URL-адресу /uploads.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(publicFilesPath),
    RequestPath = "/uploads"
});

app.MapControllers();

app.Run();