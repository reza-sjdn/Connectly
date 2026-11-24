namespace Connectly.Application.Common.Dtos;

/// <summary>Transport-agnostic upload so the Application layer never touches IFormFile.</summary>
public sealed record FileUpload(Stream Content, string FileName, string ContentType, long Length);
