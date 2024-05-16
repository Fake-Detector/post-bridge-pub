using Fake.Detection.Post.Bridge.Bll.Repositories;
using Fake.Detection.Post.Bridge.Bll.Services;
using Fake.Detection.Post.Bridge.Dal.Configure;
using Fake.Detection.Post.Bridge.Dal.Contexts;
using Fake.Detection.Post.Bridge.Dal.Repositories;
using Fake.Detection.Post.Bridge.Dal.Services;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FileOptions = Fake.Detection.Post.Bridge.Dal.Configure.FileOptions;

namespace Fake.Detection.Post.Bridge.Dal.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDal(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<DalOptions>(config.GetSection(nameof(DalOptions)));
        services.Configure<FileOptions>(config.GetSection(nameof(FileOptions)));

        services
            .AddDbContexts()
            .AddMigrations()
            .AddRepositories()
            .AddIntegration(config)
            .AddServices();

        return services;
    }

    private static IServiceCollection AddDbContexts(this IServiceCollection services)
    {
        services.AddDbContext<PostContext>((s, options) =>
        {
            var cfg = s.GetRequiredService<IOptions<DalOptions>>();
            options.UseLazyLoadingProxies().UseNpgsql(cfg.Value.ConnectionString);
        }, ServiceLifetime.Singleton);

        return services;
    }

    private static IServiceCollection AddMigrations(this IServiceCollection services)
    {
        services.AddFluentMigratorCore()
            .ConfigureRunner(rb => rb.AddPostgres()
                .WithGlobalConnectionString(s =>
                {
                    var cfg = s.GetRequiredService<IOptions<DalOptions>>();
                    return cfg.Value.ConnectionString;
                })
                .ScanIn(typeof(ServiceCollectionExtensions).Assembly).For.Migrations()
            )
            .AddLogging(lb => lb.AddFluentMigratorConsole());

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IPostRepository, PostRepository>();
        services.AddSingleton<IItemRepository, ItemRepository>();
        services.AddSingleton<IFeatureRepository, FeatureRepository>();

        return services;
    }

    private static IServiceCollection AddIntegration(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<GrpcOptions>(config.GetSection(nameof(GrpcOptions)));

        services.AddGrpcClient<NewsGetter.NewsGetter.NewsGetterClient>((provider, options) =>
        {
            options.Address = new Uri(provider.GetRequiredService<IOptions<GrpcOptions>>().Value.ServerUrl);
        });

        return services;
    }
    
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileService, FileService>();
        
        services.AddSingleton<INewsGetterService, NewsGetterService>();

        return services;
    }
}