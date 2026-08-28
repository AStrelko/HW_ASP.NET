using System.Reflection;
using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentValidation;
using HW_06;
using HW_06.Features.Common.Behaviors;
using HW_06.Helpers;
using HW_06.Middleware;
using HW_06.Models;
using HW_06.Profile;
using HW_06.Services;
using HW_06.Services.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc;
using HW_06.Storage.Configurations;
using Microsoft.Extensions.Options;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//
// Serilog
//

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId();
});

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
// Інфраструктурні сервіси
//

builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

//
// MediatR
//

// Реєструє всі команди, запити
// та їх обробники з поточної збірки.
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

//
// FluentValidation
//

// Реєструє всі FluentValidation-валідатори
// команд і запитів з поточної збірки.
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

//
// MediatR Pipeline
//

// Виконує FluentValidation перед запуском
// відповідного MediatR handler.
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

//
// JWT Options
//

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(
        builder.Configuration.GetSection(
            JwtOptions.SectionName))
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.Issuer),
        "Jwt:Issuer не налаштовано.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.Audience),
        "Jwt:Audience не налаштовано.")
    .Validate(
        options =>
            !string.IsNullOrWhiteSpace(
                options.Key),
        "Jwt:Key не налаштовано.")
    .Validate(
        options =>
            options.AccessTokenMinutes > 0,
        "Jwt:AccessTokenMinutes " +
        "повинен бути більшим за нуль.")
    .ValidateOnStart();

//
// FileStorage Options
//

builder.Services
    .AddOptions<FileStorageOptions>()
    .Bind(
        builder.Configuration.GetSection(
            FileStorageOptions.SectionName))
    .Validate(
        options =>
            options.MaxAvatarSizeMb > 0,
        "FileStorage:MaxAvatarSizeMb " +
        "повинен бути більшим за нуль.")
    .Validate(
        options =>
            options.MaxPublicDocumentSizeMb > 0,
        "FileStorage:MaxPublicDocumentSizeMb " +
        "повинен бути більшим за нуль.")
    .Validate(
        options =>
            options.MaxPrivateDocumentSizeMb > 0,
        "FileStorage:MaxPrivateDocumentSizeMb " +
        "повинен бути більшим за нуль.")
    .ValidateOnStart();

//
// JWT Authentication
//
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
        {
            var jwtOptions = jwtOptionsAccessor.Value;

            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role"
                };

            options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                        {
                            context.HandleResponse();
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            context.Response.ContentType = "application/problem+json";

                            await context.Response.WriteAsJsonAsync(new ProblemDetails
                                    {
                                        Status = StatusCodes.Status401Unauthorized,
                                        Title = "Користувач не авторизований.",
                                        Detail = "Для виконання цієї дії необхідно авторизуватися.",
                                        Instance = context.Request.Path
                                    });
                        },

                    OnForbidden = async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            context.Response.ContentType = "application/problem+json";

                            await context.Response.WriteAsJsonAsync(new ProblemDetails
                                    {
                                        Status = StatusCodes.Status403Forbidden,
                                        Title = "Доступ заборонено.",
                                        Detail = "У вас недостатньо прав для виконання цієї дії.",
                                        Instance = context.Request.Path
                                    });
                        }
                };
        });

builder.Services.AddAuthorization();

//
// Версіонування API
//

builder.Services
    .AddApiVersioning(options =>
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
// Swagger
//

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

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
            Description = "Please enter token"
        });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            });
});

var app = builder.Build();

//
// Correlation ID
//

app.UseMiddleware<CorrelationIdMiddleware>();

//
// Serilog HTTP request logging
//

app.UseSerilogRequestLogging();

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
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
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

var publicFilesPath = Path.Combine(app.Environment.ContentRootPath, "uploads", "PublicFile");

Directory.CreateDirectory(publicFilesPath);

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