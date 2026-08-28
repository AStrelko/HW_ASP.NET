namespace HW_06.Common.Constants;

/// <summary>
/// Містить підтримувані типи
/// завантажуваних файлів,
/// їх MIME-типи та сигнатури.
/// </summary>
public static class FileTypeConstants
{
    //
    // Зображення
    //

    /// <summary>
    /// Дозволені розширення зображень.
    /// </summary>
    public static readonly string[] AllowedImageExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    ];

    /// <summary>
    /// Дозволені MIME-типи зображень.
    /// </summary>
    public static readonly string[] AllowedImageMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    /// <summary>
    /// Сигнатури підтримуваних
    /// форматів зображень.
    /// </summary>
    public static readonly IReadOnlyDictionary<
        string,
        byte[][]> ImageSignatures =
        new Dictionary<string, byte[][]>(
            StringComparer.OrdinalIgnoreCase)
        {
            [".jpg"] =
            [
                [0xFF, 0xD8, 0xFF]
            ],

            [".jpeg"] =
            [
                [0xFF, 0xD8, 0xFF]
            ],

            [".png"] =
            [
                [
                    0x89,
                    0x50,
                    0x4E,
                    0x47,
                    0x0D,
                    0x0A,
                    0x1A,
                    0x0A
                ]
            ],

            [".webp"] =
            [
                [0x52, 0x49, 0x46, 0x46]
            ]
        };

    //
    // Документи
    //

    /// <summary>
    /// Дозволені розширення документів.
    /// </summary>
    public static readonly string[] AllowedDocumentExtensions =
    [
        ".pdf",
        ".docx",
        ".txt"
    ];

    /// <summary>
    /// MIME-типи документів
    /// відповідно до їх розширень.
    /// </summary>
    public static readonly IReadOnlyDictionary<
        string,
        string> DocumentMimeTypes =
        new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] =
                "application/pdf",

            [".docx"] =
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",

            [".txt"] =
                "text/plain"
        };
}