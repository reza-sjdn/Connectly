namespace Connectly.Application.Common.Interfaces;

public interface IFileStorage
{
    /// <returns>Public relative URL of the stored file, e.g. /media/posts/abc.jpg</returns>
    Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken ct = default);
    Task DeleteAsync(string relativeUrl, CancellationToken ct = default);
}
