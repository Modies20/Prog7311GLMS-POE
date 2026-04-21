using Xunit;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace GLMS.Tests.Services;

public class FileValidationTests
{
    private readonly FileValidationService _validationService;

    public FileValidationTests()
    {
        _validationService = new FileValidationService();
    }

    [Fact]
    public void IsValidFile_WithValidPdfFile_ReturnsTrue()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var fileName = "contract.pdf";
        var content = "Dummy PDF content";
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(stream.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

        // Act
        var result = _validationService.IsValidFile(mockFile.Object);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValidFile_WithExeFile_ReturnsFalse()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("malware.exe");
        mockFile.Setup(f => f.Length).Returns(1024);

        // Act
        var result = _validationService.IsValidFile(mockFile.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidFile_WithEmptyFile_ReturnsFalse()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("empty.pdf");
        mockFile.Setup(f => f.Length).Returns(0);

        // Act
        var result = _validationService.IsValidFile(mockFile.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValidFile_WithFileExceedingSizeLimit_ReturnsFalse()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("large.pdf");
        mockFile.Setup(f => f.Length).Returns(11 * 1024 * 1024); // 11 MB

        // Act
        var result = _validationService.IsValidFile(mockFile.Object);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFileValidationError_WithInvalidExtension_ReturnsCorrectMessage()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("bad.exe");
        mockFile.Setup(f => f.Length).Returns(1024);

        // Act
        var error = _validationService.GetFileValidationError(mockFile.Object);

        // Assert
        Assert.Contains("PDF", error);
        Assert.Contains("only", error.ToLower());
    }

    [Fact]
    public void GetFileValidationError_WithNullFile_ReturnsErrorMessage()
    {
        // Act
        var error = _validationService.GetFileValidationError(null);

        // Assert
        Assert.Contains("No file uploaded", error);
    }

    [Theory]
    [InlineData("document.pdf", true)]
    [InlineData("contract.PDF", true)]
    [InlineData("file.txt", false)]
    [InlineData("image.jpg", false)]
    public void IsPdfFile_VariousExtensions_ReturnsExpectedResult(string fileName, bool expected)
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(1024);

        // Act
        var result = _validationService.IsPdfFile(mockFile.Object);

        // Assert
        Assert.Equal(expected, result);
    }
}