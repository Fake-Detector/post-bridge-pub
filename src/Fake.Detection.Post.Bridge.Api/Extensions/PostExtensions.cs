using Fake.Detection.Post.Bridge.Api.Helpers;
using Fake.Detection.Post.Bridge.Bll.Models;
using Fake.Detection.Post.Bridge.Contracts;
using Google.Protobuf.WellKnownTypes;
using Enum = System.Enum;

namespace Fake.Detection.Post.Bridge.Api.Extensions;

public static class PostExtensions
{
    public static Contracts.Post ToPost(this PostInfo postInfo, UrlHelper urlHelper) =>
        new()
        {
            Id = postInfo.Id,
            DataSource = postInfo.GetDataSource(),
            AuthorId = postInfo.AuthorId,
            Items = { postInfo.ItemInfos.Select(item => item.ToItem(urlHelper)) },
            CreatedAt = Timestamp.FromDateTime(postInfo.CreatedAt),
            ExternalId = postInfo.ExternalId,
        };

    private static DataSource GetDataSource(this PostInfo postInfo) =>
        Enum.TryParse<DataSource>(postInfo.DataSource, out var dataSource)
            ? dataSource
            : throw new ArgumentOutOfRangeException(nameof(postInfo.DataSource), "Incorrect post data source type");

    private static Item ToItem(this ItemInfo itemInfo, UrlHelper urlHelper) =>
        new()
        {
            Id = itemInfo.Id.ToString(),
            PostId = itemInfo.PostId,
            Type = itemInfo.GetItemType(),
            Data = itemInfo.GetItemType() is ItemType.Text
                ? itemInfo.Text
                : itemInfo.Url ?? urlHelper.GenerateDataUrl(itemInfo.Id),
            Features = { itemInfo.FeatureInfos.Select(ToFeature) }
        };

    private static ItemType GetItemType(this ItemInfo itemInfo) =>
        Enum.TryParse<ItemType>(itemInfo.Type, out var itemType)
            ? itemType
            : throw new ArgumentOutOfRangeException(nameof(itemInfo.Type), "Incorrect item type");

    public static Feature ToFeature(this FeatureInfo featureInfo) =>
        new()
        {
            Id = featureInfo.Id,
            ItemId = featureInfo.ItemId.ToString(),
            Type = featureInfo.GetFeatureType(),
            Text = featureInfo.Text
        };

    private static FeatureType GetFeatureType(this FeatureInfo featureInfo) =>
        Enum.TryParse<FeatureType>(featureInfo.Type, out var featureType)
            ? featureType
            : throw new ArgumentOutOfRangeException(nameof(featureInfo.Type), "Incorrect feature type");
}