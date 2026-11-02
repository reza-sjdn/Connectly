using Connectly.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace Connectly.Infrastructure.Storage;

public class StorageSettings
{
    public string RootPath { get; set; } = "uploads";   // physical folder
    public string RequestPath { get; set; } = "/media"; // public URL prefix
}

public class LocalFileStorage(IOptions<StorageSettings> options) : IFileStorage
{
    private readonly StorageSettings _s = options.Value;

    public async Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken ct = default)
    {
        var safeFolder = Path.GetFileName(folder.TrimEnd('/', '\\'));          // no traversal
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var name = $"{Guid.NewGuid():N}{ext}";

        var dir = Path.Combine(Path.GetFullPath(_s.RootPath), safeFolder);
        Directory.CreateDirectory(dir);

        await using var fs = File.Create(Path.Combine(dir, name));
        await content.CopyToAsync(fs, ct);

        return $"{_s.RequestPath.TrimEnd('/')}/{safeFolder}/{name}";
    }

    public Task DeleteAsync(string relativeUrl, CancellationToken ct = default)
    {
        var relative = relativeUrl.Replace(_s.RequestPath, string.Empty).TrimStart('/');
        var root = Path.GetFullPath(_s.RootPath);
        var full = Path.GetFullPath(Path.Combine(root, relative));
        if (full.StartsWith(root, StringComparison.Ordinal) && File.Exists(full)) File.Delete(full);
        return Task.CompletedTask;
    }
}
