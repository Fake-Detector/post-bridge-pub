using Fake.Detection.Post.Bridge.Bll.Models;
using Microsoft.EntityFrameworkCore;

namespace Fake.Detection.Post.Bridge.Dal.Contexts;

public class PostContext : DbContext
{
    public PostContext(DbContextOptions<PostContext> options)
        : base(options)
    {
    }

    public DbSet<PostInfo> PostInfos { get; set; } = null!;
    public DbSet<ItemInfo> ItemInfos { get; set; } = null!;
    public DbSet<FeatureInfo> FeatureInfos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PostInfo>()
            .HasMany(post => post.ItemInfos)
            .WithOne(item => item.PostInfo)
            .HasForeignKey(item => item.PostId)
            .HasPrincipalKey(post => post.Id);

        modelBuilder.Entity<ItemInfo>()
            .HasMany(item => item.FeatureInfos)
            .WithOne(feature => feature.ItemInfo)
            .HasForeignKey(feature => feature.ItemId)
            .HasPrincipalKey(item => item.Id);
    }
}