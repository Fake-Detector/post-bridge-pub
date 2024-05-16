using Fake.Detection.Post.Bridge.Bll.Models;

namespace Fake.Detection.Post.Bridge.Bll.Services;

public interface INewsGetterService
{
    Task<(bool isSuccess, NewsContent news)> GetNews(string url, CancellationToken token);
    Task<List<NewsShortContent>> GetAllNews(string source, long page, CancellationToken token);
}