using PropertyEval.Application.Interfaces;

namespace PropertyEval.Web.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly Dictionary<string, string> ContentTypeExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var relativePath = CreateRelativePath(fileName, contentType);
        var fullPath = GetFullPath(relativePath);
        var directory = Path.GetDirectoryName(fullPath)
            ?? throw new InvalidOperationException("Upload directory could not be resolved.");

        Directory.CreateDirectory(directory);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, cancellationToken);

        return "/" + relativePath.Replace('\\', '/');
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.CompletedTask;
        }

        var relativePath = fileUrl.TrimStart('/', '\\');
        var fullPath = GetFullPath(relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string relativePath)
    {
        var webRootPath = GetWebRootPath();
        var fullPath = Path.GetFullPath(Path.Combine(
            webRootPath,
            relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar)));
        var fullRoot = Path.GetFullPath(webRootPath);

        if (!fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("File path is outside the configured upload directory.");
        }

        return fullPath;
    }

    private string GetWebRootPath()
    {
        var webRootPath = _environment.WebRootPath;

        if (!string.IsNullOrWhiteSpace(webRootPath))
        {
            return webRootPath;
        }

        return Path.Combine(_environment.ContentRootPath, "wwwroot");
    }

    private static string CreateRelativePath(string fileName, string contentType)
    {
        var segments = fileName
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var directorySegments = segments.Length > 1
            ? segments[..^1].Select(SanitizePathSegment).Where(segment => segment.Length > 0)
            : ["uploads"];
        var originalFileName = segments.Length > 0 ? segments[^1] : "image";
        var extension = GetSafeExtension(originalFileName, contentType);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";

        return string.Join('/', directorySegments.Append(storedFileName));
    }

    private static string SanitizePathSegment(string segment)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitizedChars = segment
            .Where(character => !invalidChars.Contains(character) && character is not '.' and not '/' and not '\\')
            .ToArray();

        return new string(sanitizedChars);
    }

    private static string GetSafeExtension(string fileName, string contentType)
    {
        if (ContentTypeExtensions.TryGetValue(contentType, out var contentTypeExtension))
        {
            return contentTypeExtension;
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return ContentTypeExtensions.ContainsValue(extension) ? extension : ".bin";
    }
}
