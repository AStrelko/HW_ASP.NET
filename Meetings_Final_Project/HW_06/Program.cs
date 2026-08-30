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
using HW_06.Storage.Configurations;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Diagnostics;
using System.Globalization;
using System.Threading.RateLimiting;
using System.Diagnostics;
using System.Text.Json;

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
// CORS
//

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
                         .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        });
});

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

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
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

builder.Services.AddAutoMapper(typeof(MeetingMappingProfile));

//
// Інфраструктурні сервіси
//

builder.Services.AddScoped<ITokenService, JwtTokenService>();

builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();

//
// MediatR
//

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

//
// FluentValidation
//

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

//
// MediatR Pipeline
//

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

//
// JWT Options
//

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer),
        "Jwt:Issuer не налаштовано.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Audience),
        "Jwt:Audience не налаштовано.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Key),
        "Jwt:Key не налаштовано.")
    .Validate(options => options.AccessTokenMinutes > 0,
        "Jwt:AccessTokenMinutes повинен бути більшим за нуль.")
    .ValidateOnStart();

//
// FileStorage Options
//

builder.Services
    .AddOptions<FileStorageOptions>()
    .Bind(builder.Configuration.GetSection(FileStorageOptions.SectionName))
    .Validate(
        options => options.MaxAvatarSizeMb > 0,
        "FileStorage:MaxAvatarSizeMb повинен бути більшим за нуль.")
    .Validate(
        options => options.MaxPublicDocumentSizeMb > 0,
        "FileStorage:MaxPublicDocumentSizeMb повинен бути більшим за нуль.")
    .Validate(
        options => options.MaxPrivateDocumentSizeMb > 0,
        "FileStorage:MaxPrivateDocumentSizeMb повинен бути більшим за нуль.")
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

builder.Services
    .AddOptions<JwtBearerOptions>(
        JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>(
        (options, jwtOptionsAccessor) =>
        {
            var jwtOptions =
                jwtOptionsAccessor.Value;

            options.MapInboundClaims = false;

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                jwtOptions.Key)),

                    NameClaimType =
                        JwtRegisteredClaimNames.Sub,

                    RoleClaimType = "role"
                };

            options.Events = new JwtBearerEvents
            {
                //
                // 401 — токен відсутній або недійсний.
                //

                OnChallenge = async context =>
                {
                    // Самостійно формуємо відповідь замість стандартної.
                    context.HandleResponse();

                    if (context.Response.HasStarted)
                    {
                        return;
                    }

                    context.Response.StatusCode =
                        StatusCodes.Status401Unauthorized;

                    context.Response.Headers["WWW-Authenticate"] =
                        "Bearer";

                    var problemDetails =
                        new ProblemDetails
                        {
                            Type =
                                "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.2",

                            Status =
                                StatusCodes.Status401Unauthorized,

                            Title =
                                "Користувач не авторизований.",

                            Detail =
                                "Для виконання цієї дії необхідний дійсний токен доступу. Виконайте вхід і повторіть запит.",

                            Instance =
                                context.Request.Path
                        };

                    problemDetails.Extensions["traceId"] =
                        Activity.Current?.Id ??
                        context.HttpContext.TraceIdentifier;

                    await context.Response.WriteAsJsonAsync(
                        problemDetails,
                        options: (JsonSerializerOptions?)null,
                        contentType: "application/problem+json",
                        cancellationToken:
                            context.HttpContext.RequestAborted);
                },

                //
                // 403 — користувач автентифікований,
                // але не має необхідних прав.
                //

                OnForbidden = async context =>
                {
                    if (context.Response.HasStarted)
                    {
                        return;
                    }

                    context.Response.StatusCode =
                        StatusCodes.Status403Forbidden;

                    var problemDetails =
                        new ProblemDetails
                        {
                            Type =
                                "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.4",

                            Status =
                                StatusCodes.Status403Forbidden,

                            Title =
                                "Доступ заборонено.",

                            Detail =
                                "У вас недостатньо прав для виконання цієї дії.",

                            Instance =
                                context.Request.Path
                        };

                    problemDetails.Extensions["traceId"] =
                        Activity.Current?.Id ??
                        context.HttpContext.TraceIdentifier;

                    await context.Response.WriteAsJsonAsync(
                        problemDetails,
                        options: (JsonSerializerOptions?)null,
                        contentType: "application/problem+json",
                        cancellationToken:
                            context.HttpContext.RequestAborted);
                }
            };
        });

builder.Services.AddAuthorization();

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
// Swagger
//

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddSwaggerGen(options =>
{
    var xmlFile =$"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);

    //
    // Swagger — JWT Bearer
    //

    options.AddSecurityDefinition( "Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            BearerFormat ="JWT",
            Scheme ="bearer",
            Description ="Please enter token"
        });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
            });
});

//
// Rate limiting
//

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Загальне обмеження для запитів до API.
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(ipAddress,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

    // Окреме обмеження для входу.
    options.AddPolicy("Login", context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(5),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        });

    // Окреме обмеження для реєстрації.
    options.AddPolicy("Register",context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                });
        });

    // Єдина відповідь при перевищенні ліміту.
    options.OnRejected = async (context, cancellationToken) =>
        {
            var httpContext = context.HttpContext;
            httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                httpContext.Response.Headers["Retry-After"] =
                    Math.Ceiling(retryAfter.TotalSeconds)
                        .ToString(CultureInfo.InvariantCulture);
            }

            var problem =
                new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Перевищено ліміт запитів.",
                    Detail = "Забагато запитів за короткий час. Спробуйте повторити запит пізніше.",
                    Instance = httpContext.Request.Path
                };

            problem.Extensions["traceId"] =
                Activity.Current?.Id
                ?? httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(
                problem,
                options:
                    (System.Text.Json.JsonSerializerOptions?)null,
                contentType:
                    "application/problem+json",
                cancellationToken:
                    cancellationToken);
        };
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

if (app.Configuration.GetValue<bool>("Seed:Enabled"))
{
    using var scope = app.Services.CreateScope();
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
            options.SwaggerEndpoint($"/swagger/"
                                    + $"{description.GroupName}"
                                    + "/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}
else
{
    //
    // HSTS використовується лише у Production.
    //

    app.UseHsts();
}

//
// HTTPS
//

app.UseHttpsRedirection();

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
// Routing
//

app.UseRouting();

//
// CORS
//

app.UseCors("Frontend");

//
// Rate limiting
//

app.UseRateLimiter();

//
// Identity
//

app.UseAuthentication();
app.UseAuthorization();

//
// Контролери
//

app.MapControllers();

app.Run();