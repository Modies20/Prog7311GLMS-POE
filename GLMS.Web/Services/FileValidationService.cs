using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

public class FileValidationService : IFileValidationService
{
    private readonly string[] _allowedExtensions = { ".pdf" };
    private const int DEFAULT_MAX_SIZE_MB = 10;

    public bool IsValidFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return false;

        return IsPdfFile(file) && IsWithinSizeLimit(file);
    }

    public bool IsPdfFile(IFormFile? file)
    {
        if (file == null)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    public bool IsWithinSizeLimit(IFormFile? file, int maxSizeMB = DEFAULT_MAX_SIZE_MB)
    {
        if (file == null)
            return false;

        var maxSizeBytes = maxSizeMB * 1024L * 1024L;
        return file.Length <= maxSizeBytes;
    }

    public string GetFileValidationError(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return "No file uploaded. Please select a PDF document.";

        if (!IsPdfFile(file))
            return "Invalid file type. Only PDF (.pdf) files are allowed for contract agreements.";

        if (!IsWithinSizeLimit(file))
            return $"File size exceeds the {DEFAULT_MAX_SIZE_MB}MB limit. Please compress or split the file.";

        return string.Empty;
    }
}
