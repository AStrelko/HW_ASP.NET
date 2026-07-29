namespace HW_06.Models.Files;

public class StoredFileResult
{
    public required string StoredFileName { get; init; }

    public required string RelativePath { get; init; }

    public long Size { get; init; }

    public required string ContentType { get; init; }
}