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
using HW_06.Validators.MeetingValid;
using HW_06.Validators.ParticipantValid;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//
// Controllers
//

builder.Services.AddControllers();

//
// Database
//

builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

//
// AutoMapper
//

builder.Services.AddAutoMapper(typeof(MeetingMappingProfile));

//
// Services
//

builder.Services.AddScoped<IMeetingService, MeetingService>();
builder.Services.AddScoped<IParticipantService, ParticipantService>();

//
// Meeting validators
//

builder.Services.AddScoped<
    IValidator<MeetingCreateDTO>,
    MeetingCreateValidator>();

builder.Services.AddScoped<
    IValidator<MeetingUpdateDTO>,
    MeetingUpdateValidator>();

builder.Services.AddScoped<
    IValidator<MeetingPartialUpdateDTO>,
    MeetingPartialUpdateValidator>();

//
// Participant validators
//

builder.Services.AddScoped<
    IValidator<ParticipantCreateDTO>,
    ParticipantCreateValidator>();

builder.Services.AddScoped<
    IValidator<ParticipantUpdateDTO>,
    ParticipantUpdateValidator>();

builder.Services.AddScoped<
    IValidator<ParticipantPartialUpdateDTO>,
    ParticipantPartialUpdateValidator>();

//
// API Versioning
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
    var xmlFile =
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath =
        Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

//
// Seed data
//

using (var scope = app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider.GetRequiredService<DataContext>();

    SeedData.Initialize(context);
}

//
// HTTP pipeline
//

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        var provider =
            app.Services
                .GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description
                 in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint(
                $"/swagger/{description.GroupName}/swagger.json",
                description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();