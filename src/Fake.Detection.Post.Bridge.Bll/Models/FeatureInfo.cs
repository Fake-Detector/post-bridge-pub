using System.ComponentModel.DataAnnotations.Schema;

namespace Fake.Detection.Post.Bridge.Bll.Models;

[Table("feature")]
public class FeatureInfo
{
    [Column("id")] public long Id { get; set; }
    [Column("item_id")] public Guid ItemId { get; set; }
    [Column("type")] public string Type { get; set; } = null!;
    [Column("text")] public string Text { get; set; } = null!;
    public virtual ItemInfo ItemInfo { get; set; } = null!;
}