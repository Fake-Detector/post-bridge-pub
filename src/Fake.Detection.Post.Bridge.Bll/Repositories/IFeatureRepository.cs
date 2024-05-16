namespace Fake.Detection.Post.Bridge.Bll.Repositories;

public interface IFeatureRepository
{
    Task AddFeatureAsync(Guid itemId, string type, string text, CancellationToken token);
}