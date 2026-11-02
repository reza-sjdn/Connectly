using Connectly.Application.Common.Models;
using FluentValidation;

namespace Connectly.Application.Common.Helpers;

public static class ImageFile
{
    public const long MaxBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    public static bool IsAllowedContentType(string contentType) => Extensions.ContainsKey(contentType);

    /// <summary>Extension is derived from the content type, never from the client-supplied file name.</summary>
    public static string ExtensionFor(string contentType) => Extensions[contentType];

    /// <summary>Checks magic bytes (JPEG / PNG / WebP) so a renamed .exe or .html is rejected.</summary>
    public static async Task<bool> HasValidSignatureAsync(Stream stream, CancellationToken ct)
    {
        var buf = new byte[12];
        var read = 0;
        while (read < buf.Length)
        {
            var n = await stream.ReadAsync(buf.AsMemory(read), ct);
            if (n == 0) break;
            read += n;
        }
        if (stream.CanSeek) stream.Position = 0;
        if (read < 12) return false;

        var jpeg = buf[0] == 0xFF && buf[1] == 0xD8 && buf[2] == 0xFF;
        var png = buf[0] == 0x89 && buf[1] == 0x50 && buf[2] == 0x4E && buf[3] == 0x47;
        var webp = buf[0] == 'R' && buf[1] == 'I' && buf[2] == 'F' && buf[3] == 'F'
                   && buf[8] == 'W' && buf[9] == 'E' && buf[10] == 'B' && buf[11] == 'P';
        return jpeg || png || webp;
    }
}

public class ImageUploadValidator : AbstractValidator<FileUpload>
{
    public ImageUploadValidator()
    {
        RuleFor(x => x.Length).GreaterThan(0).WithMessage("The file is empty.")
            .LessThanOrEqualTo(ImageFile.MaxBytes).WithMessage("The file must be 5 MB or smaller.");
        RuleFor(x => x.ContentType).Must(ImageFile.IsAllowedContentType)
            .WithMessage("Only JPEG, PNG or WebP images are allowed.");
    }
}
