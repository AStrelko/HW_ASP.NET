namespace HW_06;
using Asp.Versioning.ApiExplorer; // Тип IApiVersionDescriptionProvider — постачальник інформації про версії API
using Microsoft.Extensions.Options; // Тип IConfigureOptions<T> — механізм конфігурування options-об'єктів
using Microsoft.OpenApi; // Тип OpenApiInfo — опис метаданих Swagger-документа (назва, версія тощо)
using Swashbuckle.AspNetCore.SwaggerGen; // Тип SwaggerGenOptions — налаштування генератора Swagger

/// <summary>
/// Налаштовує окремі Swagger-документи для кожної підтримуваної версії API.
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>

{
    /// <summary>
    /// Постачальник інформації
    /// про доступні версії API.
    /// </summary>
    private readonly IApiVersionDescriptionProvider _provider;

    /// <summary>
    /// Ініціалізує новий екземпляр
    /// класу <see cref="ConfigureSwaggerOptions"/>.
    /// </summary>
    /// <param name="provider">
    /// Постачальник описів версій API.
    /// </param>
    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;

    }

    /// <summary>
    /// Реєструє Swagger-документ
    /// для кожної знайденої версії API.
    /// </summary>
    /// <param name="options">
    /// Параметри генератора Swagger.
    /// </param>
    public void Configure(SwaggerGenOptions options)
    {
        // Реєструє окремий Swagger-документ для кожної версії API.
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