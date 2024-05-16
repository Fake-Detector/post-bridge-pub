using Fake.Detection.Post.Bridge.Bll.Models;
using Fake.Detection.Post.Bridge.Bll.Repositories;
using Fake.Detection.Post.Bridge.Dal.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Fake.Detection.Post.Bridge.Dal.Repositories;

public class FeatureRepository : IFeatureRepository
{
    private readonly PostContext _context;

    public FeatureRepository(PostContext context)
    {
        _context = context;
    }

    public async Task AddFeatureAsync(Guid itemId, string type, string text, CancellationToken token)
    {
        var itemInfo = await _context.ItemInfos.FirstOrDefaultAsync(x => x.Id == itemId, token);

        if (itemInfo is null)
            return;

        await _context.FeatureInfos.AddAsync(new FeatureInfo { ItemId = itemInfo.Id, Type = type, Text = text }, token);

        await _context.SaveChangesAsync(token);
    }
}