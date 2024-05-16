using Microsoft.AspNetCore.StaticFiles;

namespace Fake.Detection.Post.Bridge.Api.Extensions;

public static class FileExtensions
{
    private static readonly FileExtensionContentTypeProvider TypeProvider = new();

    private const string BaseMimeType = "application/octet-stream";

    public static string GetMimeType(this string filePath) =>
        TypeProvider.TryGetContentType(filePath, out var contentType) ? contentType : BaseMimeType;
}