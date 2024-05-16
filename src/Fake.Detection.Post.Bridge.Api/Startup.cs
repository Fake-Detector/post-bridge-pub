using System.Text;
using Common.Library.Kafka.Common.Extensions;
using Common.Library.Kafka.Consumer.Extensions;
using Common.Library.Kafka.Producer.Extensions;
using Fake.Detection.Post.Bridge.Api.Configure;
using Fake.Detection.Post.Bridge.Api.Consumer;
using Fake.Detection.Post.Bridge.Api.Helpers;
using Fake.Detection.Post.Bridge.Api.Interceptors;
using Fake.Detection.Post.Bridge.Api.Services;
using Fake.Detection.Post.Bridge.Bll.Extensions;
using Fake.Detection.Post.Bridge.Contracts;
using Fake.Detection.Post.Bridge.Dal.Extensions;
using Fake.Detection.Post.Monitoring.Client.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;

namespace Fake.Detection.Post.Bridge.Api;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<PostProducerOptions>(_configuration.GetSection(nameof(PostProducerOptions)));
        services.AddCommonKafka(_configuration);
        services.AddConsumerHandler<Feature, ConsumerFeatureOptions, ConsumerHandler>(_configuration);
        services.AddProducerHandler<Contracts.Post>();
        services.AddMonitoring(_configuration);

        services.Configure<UrlOptions>(_configuration.GetSection(nameof(UrlOptions)));
        services.AddSingleton<UrlHelper>();

        services.AddGrpcReflection();
        services.AddGrpc(o => { o.Interceptors.Add<ExceptionInterceptor>(); });

        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddBll();
        services.AddDal(_configuration);
        
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration.GetSection("JWTOptions:Issuer").Value,
                    ValidAudience = _configuration.GetSection("JWTOptions:Audience").Value,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_configuration.GetSection("JWTOptions:SecretKey").Value!))
                };
            });
            
        services.AddAuthorization();
    }

    public void Configure(
        IHostEnvironment environment,
        IApplicationBuilder app)
    {
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();
        
        app.UseEndpoints(o =>
        {
            o.MapGrpcService<BridgeService>();
            o.MapGrpcReflectionService();
            o.MapControllers();
        });

        app.UseSwagger();
        app.UseSwaggerUI();
    }
}