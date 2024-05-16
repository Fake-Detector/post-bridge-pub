namespace Fake.Detection.Post.Bridge.Bll.Services;

public interface IFileService
{
    Task<string> SaveFileAsync(byte[] content, string fileName, string fileFormat, CancellationToken token);
}