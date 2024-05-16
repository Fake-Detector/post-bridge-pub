using Fake.Detection.Post.Bridge.Bll.Models;
using Fake.Detection.Post.Bridge.Bll.Repositories;
using Fake.Detection.Post.Bridge.Dal.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fake.Detection.Post.Bridge.Dal.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly PostContext _context;

    public ItemRepository(PostContext context)
    {
        _context = context;
    }

    public Task<ItemInfo?> GetAsync(Guid itemId, CancellationToken token) =>
        _context.ItemInfos.FirstOrDefaultAsync(x => x.Id == itemId, token);

    public async Task AddItemAsync(long postId, string? text, string? filePath, string? url, Guid? id, string type,
        CancellationToken token)
    {
        var post = await _context.PostInfos.FirstOrDefaultAsync(x => x.Id == postId, cancellationToken: token);

        if (post is null)
            return;

        var item = new ItemInfo
        {
            Id = id ?? Guid.NewGuid(),
            PostId = post.Id,
            Text = text,
            FilePath = filePath,
            Url = url,
            Type = type,
            CreatedAt = DateTime.UtcNow,
        };

        await _context.ItemInfos.AddAsync(item, token);
        await _context.SaveChangesAsync(token);
    }
}