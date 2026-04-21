using Microsoft.AspNetCore.Http;

namespace GLMS.Web.Services;

public interface IFileValidationService
{
    bool IsValidFile(IFormFile? file);
    string GetFileValidationError(IFormFile? file);
    bool IsPdfFile(IFormFile? file);
    bool IsWithinSizeLimit(IFormFile? file, int maxSizeMB = 10);
}