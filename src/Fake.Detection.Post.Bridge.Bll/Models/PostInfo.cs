using System.ComponentModel.DataAnnotations.Schema;

namespace Fake.Detection.Post.Bridge.Bll.Models;

[Table("post")]
public class PostInfo
{ 
    [Column("id")] public long Id { get; set; }
    [Column("data_source")] public string DataSource { get; set; } = null!;
    [Column("author_id")] public string AuthorId { get; set; }
    [Column("external_id")] public string ExternalId { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    public virtual List<ItemInfo> ItemInfos { get; set; } = new();
}