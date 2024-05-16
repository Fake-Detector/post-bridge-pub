using FluentMigrator;

namespace Telegram.Data.Collector.DAL.Migrations;

[Migration(20231508, TransactionBehavior.None)]
public class InitSchema : Migration {
    public override void Up()
    {
        Create.Table("post")
            .WithColumn("id").AsInt64().PrimaryKey("post_pk").Identity()
            .WithColumn("data_source").AsString().NotNullable()
            .WithColumn("author_id").AsString().NotNullable()
            .WithColumn("external_id").AsString().NotNullable().Unique()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();
        
        Create.Table("item")
            .WithColumn("id").AsGuid().PrimaryKey("item_pk")
            .WithColumn("post_id").AsInt64().NotNullable()
            .WithColumn("text").AsString().Nullable()
            .WithColumn("file_path").AsString().Nullable()
            .WithColumn("url").AsString().Nullable()
            .WithColumn("type").AsString().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable();

        Create.Table("feature")
            .WithColumn("id").AsInt64().PrimaryKey("feature_pk").Identity()
            .WithColumn("item_id").AsGuid().NotNullable()
            .WithColumn("type").AsString().NotNullable()
            .WithColumn("text").AsString().NotNullable();

        Create.ForeignKey()
            .FromTable("item").ForeignColumn("post_id")
            .ToTable("post").PrimaryColumn("id");
        
        Create.ForeignKey()
            .FromTable("feature").ForeignColumn("item_id")
            .ToTable("item").PrimaryColumn("id");
    }

    public override void Down()
    {
        Delete.Table("post");
        Delete.Table("item");
        Delete.Table("feature");
    }
}