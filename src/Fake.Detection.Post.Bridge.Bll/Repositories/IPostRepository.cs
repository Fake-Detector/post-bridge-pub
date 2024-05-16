using Fake.Detection.Post.Bridge.Bll.Models;

namespace Fake.Detection.Post.Bridge.Bll.Repositories;

public interface IPostRepository
{
    Task<long> CreateAsync(string authorId, string dataSource, string? externalId, CancellationToken token);

    Task<PostInfo?> GetAsync(string externalId, CancellationToken token);
    
    Task<PostInfo?> GetAsync(long id, CancellationToken token);

    Task<List<PostInfo>> GetAllAsync(string authorId, string dataSource, CancellationToken token);
}