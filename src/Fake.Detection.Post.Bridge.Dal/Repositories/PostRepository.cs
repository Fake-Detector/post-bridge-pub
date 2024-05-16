using Fake.Detection.Post.Bridge.Bll.Models;
using Fake.Detection.Post.Bridge.Bll.Repositories;
using Fake.Detection.Post.Bridge.Dal.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fake.Detection.Post.Bridge.Dal.Repositories;

public class PostRepository : IPostRepository
{
    private readonly PostContext _context;

    public PostRepository(PostContext context)
    {
        _context = context;
    }

    public async Task<long> CreateAsync(string authorId, string dataSource, string? externalId, CancellationToken token)
    {
        var item = await _context.PostInfos.AddAsync(
            new PostInfo
            {
                AuthorId = authorId, DataSource = dataSource, ExternalId = "", CreatedAt = DateTime.UtcNow
            },
            token);

        await _context.SaveChangesAsync(token);

        item.Entity.ExternalId = !string.IsNullOrWhiteSpace(externalId) ? externalId : item.Entity.Id.ToString();

        await _context.SaveChangesAsync(token);

        return item.Entity.Id;
    }

    public Task<PostInfo?> GetAsync(string externalId, CancellationToken token) =>
        _context.PostInfos.FirstOrDefaultAsync(x => x.ExternalId == externalId, cancellationToken: token);

    public Task<PostInfo?> GetAsync(long id, CancellationToken token) =>
        _context.PostInfos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: token);

    public Task<List<PostInfo>> GetAllAsync(string authorId, string dataSource, CancellationToken token) =>
        _context.PostInfos.Where(x => x.AuthorId == authorId && x.DataSource == dataSource).ToListAsync(token);
}