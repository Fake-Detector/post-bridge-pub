using Fake.Detection.Post.Bridge.Bll.Models;
using Fake.Detection.Post.Bridge.Bll.Services;
using NewsGetter;

namespace Fake.Detection.Post.Bridge.Dal.Services;

public class NewsGetterService : INewsGetterService
{
    private readonly NewsGetter.NewsGetter.NewsGetterClient _client;

    public NewsGetterService(NewsGetter.NewsGetter.NewsGetterClient client) => _client = client;

    public async Task<(bool isSuccess, NewsContent news)> GetNews(string url, CancellationToken token)
    {
        var request = new GetNewsContentRequest
        {
            Url = url,
            Scraper = Site.AutoDetect
        };

        var response = await _client.GetNewsContentAsync(request, cancellationToken: token);

        if (response is null)
            throw new ArgumentNullException(nameof(response), "Response is null");

        var isSuccess = response.IsSuccess;
        var content = new NewsContent(
            Url: response.Url,
            Title: response.Title,
            Content: response.Content,
            Images: response.Images.ToList(),
            SourceType: response.SourceType.ToString()
        );

        return new ValueTuple<bool, NewsContent>(isSuccess, content);
    }

    public async Task<List<NewsShortContent>> GetAllNews(string source, long page, CancellationToken token)
    {
        var request = new GetListNewsRequest
        {
            SourceType = Enum.TryParse<Site>(source, out var siteType) ? siteType : Site.AutoDetect,
            Page = page
        };

        var response = await _client.GetListNewsAsync(request, cancellationToken: token);

        if (response is null)
            throw new ArgumentNullException(nameof(response), "Response is null");

        return response.News.Select(it => new NewsShortContent(it.Url, it.Content)).ToList();
    }
}