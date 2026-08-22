using System.Reflection;
using System.Text;
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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

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
builder.Services.AddAutoMapper(typeof(MeetingMappingProfile));

//
// Сервіси
//

builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IRoleService, RoleService>();

//
// JWT Authentication
//

var jwt = builder.Configuration.GetSection("Jwt");
var jwtIssuer = jwt["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer не налаштовано.");
var jwtAudience = jwt["Audience"] ?? throw new InvalidOperationException("Jwt:Audience не налаштовано.");
var jwtKey = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key не налаштовано.");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

                NameClaimType = JwtRegisteredClaimNames.Sub,

                RoleClaimType = "role"
            };
    });
builder.Services.AddAuthorization();

//
// FluentValidation
//

// Реєструє всі FluentValidation-валідатори
// зі збірки застосунку.
builder.Services.AddValidatorsFromAssemblyContaining<MeetingCreateValidator>();

// Реєструє універсальний фільтр валідації DTO.
builder.Services.AddScoped(typeof(ValidationFilter<>));

//
// Версіонування API
//

builder.Services.AddApiVersioning(options =>
    {
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
//  Swagger — загальні налаштування
//

builder.Services.ConfigureOptions<
    ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
    
    //
    // Swagger — JWT Bearer
    //
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer",
        Description = "Please enter token",
    });
 
    options.AddSecurityRequirement(doc=>new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", doc)] = new List<string>()
    });
   
});

var app = builder.Build();

//
// Початкове наповнення бази даних
//

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    await SeedData.InitializeAsync(context, userManager, roleManager);
}

//
// Middleware глобальної обробки винятків
//

app.UseMiddleware<ExceptionHandlingMiddleware>();

//
// Middleware технічного обслуговування
//

app.UseMiddleware<MaintenanceMiddleware>();

//
// Swagger
//

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
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

var publicFilesPath = Path.Combine(app.Environment.ContentRootPath, "uploads", "PublicFile");

// Створює каталог публічних файлів,
// якщо він ще не існує.
Directory.CreateDirectory(publicFilesPath);

// Надає доступ до публічних файлів
// через URL-адресу /uploads.
app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(publicFilesPath),
        RequestPath = "/uploads"
    });

//
// Контролери
//

app.MapControllers();

app.Run();