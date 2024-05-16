using Common.Library.Kafka.Producer.Interfaces;
using Fake.Detection.Post.Bridge.Api.Configure;
using Fake.Detection.Post.Bridge.Api.Extensions;
using Fake.Detection.Post.Bridge.Api.Helpers;
using Fake.Detection.Post.Bridge.Bll.Commands;
using Fake.Detection.Post.Bridge.Contracts;
using Fake.Detection.Post.Monitoring.Client.Services.Interfaces;
using Fake.Detection.Post.Monitoring.Messages;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Fake.Detection.Post.Bridge.Api.Services;

[Authorize]
public class BridgeService : PostBridgeService.PostBridgeServiceBase
{
    private readonly IMediator _mediator;
    private readonly UrlHelper _urlHelper;
    private readonly IProducerHandler<Contracts.Post> _postProducer;
    private readonly IMonitoringClient _monitoringClient;
    private readonly IOptionsMonitor<PostProducerOptions> _postProducerOptions;

    public BridgeService(
        IMediator mediator,
        UrlHelper urlHelper,
        IProducerHandler<Contracts.Post> postProducer,
        IMonitoringClient monitoringClient,
        IOptionsMonitor<PostProducerOptions> postProducerOptions)
    {
        _mediator = mediator;
        _urlHelper = urlHelper;
        _postProducer = postProducer;
        _monitoringClient = monitoringClient;
        _postProducerOptions = postProducerOptions;
    }

    public override async Task<Empty> ProcessPost(ProcessPostRequest request, ServerCallContext context)
    {
        var postInfo = await _mediator.Send(new GetPostCommand(request.PostId), context.CancellationToken);

        var post = postInfo.ToPost(_urlHelper);

        var topic = _postProducerOptions.CurrentValue.TopicName;

        await _postProducer.Produce(topic, post, context.CancellationToken);

        await _monitoringClient.SendToMonitoring(MonitoringType.Info, JsonConvert.SerializeObject(post));

        return new Empty();
    }

    public override async Task<CreatePostResponse> CreatePost(CreatePostRequest request, ServerCallContext context)
    {
        var postId = await _mediator.Send(
            new CreatePostCommand(request.AuthorId, request.Source.ToString(), request.ExternalId),
            context.CancellationToken);

        return new CreatePostResponse { PostId = postId };
    }

    public override async Task<GetPostResponse> GetPost(GetPostRequest request, ServerCallContext context)
    {
        var postInfo = await _mediator.Send(
            new GetPostCommand(request.PostId, request.ExternalId, request.UseExternalId),
            context.CancellationToken);

        var response = new GetPostResponse
        {
            Post = postInfo.ToPost(_urlHelper)
        };

        return response;
    }

    public override async Task<Empty> ProcessItem(ProcessItemRequest request, ServerCallContext context)
    {
        var postInfo = await _mediator.Send(new GetPostCommand(request.PostId), context.CancellationToken);

        var post = postInfo.ToPost(_urlHelper);
        var item = post.Items.FirstOrDefault(it => it.Id == request.ItemId);

        if (item is null)
            return new Empty();

        post.Items.Clear();
        post.Items.Add(item);

        var topic = _postProducerOptions.CurrentValue.TopicName;

        await _postProducer.Produce(topic, post, context.CancellationToken);

        await _monitoringClient.SendToMonitoring(MonitoringType.Info, JsonConvert.SerializeObject(post));

        return new Empty();
    }

    public override async Task<SendPostItemResponse> SendPostItem(IAsyncStreamReader<SendPostItemRequest> requestStream,
        ServerCallContext context)
    {
        ItemType? type = null;
        string? format = null;
        long? postId = null;

        using var storageStream = new MemoryStream();

        try
        {
            await foreach (var chunk in requestStream.ReadAllAsync())
            {
                type ??= chunk.Item.MetaData.Type;
                format ??= chunk.Item.MetaData.Format;
                postId ??= chunk.Item.PostId;

                await storageStream.WriteAsync(chunk.Item.Chunk.ToArray());
            }

            storageStream.Seek(0, SeekOrigin.Begin);

            Guid? id;

            switch (type)
            {
                case ItemType.Text:
                    id = await HandleText(postId ?? -1, storageStream, context.CancellationToken);
                    break;
                case ItemType.Image:
                case ItemType.Audio:
                case ItemType.Video:
                    id = await HandleMediaFile(postId ?? -1, format, type.Value, storageStream,
                        context.CancellationToken);
                    break;
                case ItemType.ImageUrl:
                case ItemType.AudioUrl:
                case ItemType.VideoUrl:
                    id = await HandleMediaUrl(postId ?? -1, type.Value, storageStream, context.CancellationToken);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new SendPostItemResponse
            {
                Result = RequestResult.Ok,
                ItemId = id.ToString() ?? string.Empty
            };
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            return new SendPostItemResponse { Result = RequestResult.Fail };
        }
    }

    private async Task<Guid> HandleText(long postId, Stream stream, CancellationToken token)
    {
        var guid = Guid.NewGuid();
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync(token);

        await _mediator.Send(new AddItemCommand(postId, Id: guid, Text: text, Type: ItemType.Text.ToString()), token);

        return guid;
    }

    private async Task<Guid> HandleMediaFile(long postId, string? format, ItemType type, MemoryStream stream,
        CancellationToken token)
    {
        var guid = Guid.NewGuid();
        var path = await _mediator.Send(new SaveMediaCommand(stream.ToArray(), guid.ToString(), format ?? "bytes"),
            token);

        await _mediator.Send(new AddItemCommand(postId, FilePath: path, Id: guid, Type: type.ToString()),
            token);

        return guid;
    }

    private async Task<Guid> HandleMediaUrl(long postId, ItemType type, Stream stream, CancellationToken token)
    {
        var guid = Guid.NewGuid();
        using var reader = new StreamReader(stream);
        var url = await reader.ReadToEndAsync(token);

        await _mediator.Send(new AddItemCommand(postId, Url: url, Id: guid, Type: type.ToString()), token);

        return guid;
    }

    public override async Task<GetPostHistoryResponse> GetPostHistory(GetPostHistoryRequest request,
        ServerCallContext context)
    {
        var postsInfo = await _mediator.Send(new GetHistoryCommand(request.AuthorId, request.Source.ToString()),
            context.CancellationToken);

        var response = new GetPostHistoryResponse
        {
            Posts = { postsInfo.Select(it => it.ToPost(_urlHelper)) }
        };

        return response;
    }

    public override async Task<CheckNewsResponse> CheckNews(CheckNewsRequest request, ServerCallContext context)
    {
        var (isSuccess, isNew, postInfo) = await _mediator.Send(
            new CheckArticleCommand(request.Url, request.Site.ToString(), DataSource.News.ToString()),
            context.CancellationToken);

        if (isNew && postInfo is not null)
        {
            var post = postInfo.ToPost(_urlHelper);

            var topic = _postProducerOptions.CurrentValue.TopicName;

            await _postProducer.Produce(topic, post, context.CancellationToken);

            await _monitoringClient.SendToMonitoring(MonitoringType.Info, JsonConvert.SerializeObject(post));
        }

        return new CheckNewsResponse
        {
            IsSuccess = isSuccess,
            PostId = postInfo?.Id ?? -1,
        };
    }

    public override async Task<GetNewsResponse> GetNews(GetNewsRequest request, ServerCallContext context)
    {
        var postsInfo = await _mediator.Send(new GetHistoryCommand(request.Site.ToString(), request.Source.ToString()),
            context.CancellationToken);

        var response = new GetNewsResponse
        {
            Posts = { postsInfo.Select(it => it.ToPost(_urlHelper)) }
        };

        return response;
    }

    public override async Task<GetAllNewsResponse> GetAllNews(GetAllNewsRequest request, ServerCallContext context)
    {
        var news = await _mediator.Send(new GetAllNews(request.Site.ToString(), request.Page),
            context.CancellationToken);

        var response = new GetAllNewsResponse
        {
            News =
            {
                news.Select(it => new ShortNews
                {
                    Url = it.Url,
                    Content = it.Content
                })
            }
        };

        return response;
    }
}