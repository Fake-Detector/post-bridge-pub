using Fake.Detection.Post.Bridge.Bll.Services;
using Microsoft.Extensions.Options;

namespace Fake.Detection.Post.Bridge.Dal.Services;

public class FileService : IFileService
{
    private readonly IOptionsMonitor<Configure.FileOptions> _optionsMonitor;

    public FileService(IOptionsMonitor<Configure.FileOptions> optionsMonitor) => _optionsMonitor = optionsMonitor;

    public async Task<string> SaveFileAsync(byte[] content, string fileName, string fileFormat, CancellationToken token)
    {
        var directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _optionsMonitor.CurrentValue.BasePath);
        var path = Path.Combine(directory, $"{fileName}.{fileFormat}");

        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        await using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        await fs.WriteAsync(content, token);

        return path;
    }
}