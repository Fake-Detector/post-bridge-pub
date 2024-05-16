using Microsoft.Extensions.Options;

namespace Fake.Detection.Post.Bridge.Api.Helpers;

public class UrlHelper
{
    private readonly IOptionsMonitor<UrlOptions> _options;

    public UrlHelper(IOptionsMonitor<UrlOptions> options)
    {
        _options = options;
    }

    public string GenerateDataUrl(Guid guid)
    {
        var options = _options.CurrentValue;
        return $"{options.BaseUrl}/{options.DataUrl}/{guid.ToString()}";
    }
}