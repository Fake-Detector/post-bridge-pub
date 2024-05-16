using Fake.Detection.Post.Bridge.Bll.Models;

namespace Fake.Detection.Post.Bridge.Bll.Repositories;

public interface IItemRepository
{
    Task<ItemInfo?> GetAsync(Guid itemId, CancellationToken token);

    Task AddItemAsync(long postId, string? text, string? filePath, string? url, Guid? id, string type,
        CancellationToken token);
}