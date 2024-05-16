using System.ComponentModel.DataAnnotations.Schema;

namespace Fake.Detection.Post.Bridge.Bll.Models;

[Table("item")]
public class ItemInfo
{
    [Column("id")] public Guid Id { get; set; }
    [Column("post_id")] public long PostId { get; set; }
    [Column("text")] public string? Text { get; set; }
    [Column("file_path")] public string? FilePath { get; set; }
    [Column("url")] public string? Url { get; set; }
    [Column("type")] public string Type { get; set; } = null!;
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    public virtual List<FeatureInfo> FeatureInfos { get; set; } = new();
    public virtual PostInfo PostInfo { get; set; } = null!;
}