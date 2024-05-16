using Microsoft.Extensions.DependencyInjection;

namespace Fake.Detection.Post.Bridge.Bll.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        services.AddMediatR(c => c.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        
        return services;
    }
}