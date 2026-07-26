namespace HW_06;
using Asp.Versioning.ApiExplorer; // Тип IApiVersionDescriptionProvider — постачальник інформації про версії API
using Microsoft.Extensions.Options; // Тип IConfigureOptions<T> — механізм конфігурування options-об'єктів
using Microsoft.OpenApi; // Тип OpenApiInfo — опис метаданих Swagger-документа (назва, версія тощо)
using Swashbuckle.AspNetCore.SwaggerGen; // Тип SwaggerGenOptions — налаштування генератора Swagger

//клас реалізує інтерфейс якй сам знаходе версіі і викликає їх конфігурацію коли до ней звертаються
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>

{
    // поле для збереження провайдера версіі
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;

    }

    public void Configure(SwaggerGenOptions options)
    {
        //перебераю всі версіі які знайшлись
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            //регеструю новий свагер документ до кожной версіі
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "myApp",//назва застосунку
                Version = description.ApiVersion.ToString()//номер версіі
            });
        }
    }
}