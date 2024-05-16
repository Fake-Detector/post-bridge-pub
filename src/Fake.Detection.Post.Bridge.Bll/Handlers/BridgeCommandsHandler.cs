using Fake.Detection.Post.Bridge.Bll.Commands;
using Fake.Detection.Post.Bridge.Bll.Models;
using Fake.Detection.Post.Bridge.Bll.Repositories;
using Fake.Detection.Post.Bridge.Bll.Services;
using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Handlers;

public class BridgeCommandsHandler :
    IRequestHandler<GetPostCommand, PostInfo>,
    IRequestHandler<CreatePostCommand, long>,
    IRequestHandler<AddItemCommand, Unit>,
    IRequestHandler<AddFeatureCommand, Unit>,
    IRequestHandler<GetItemCommand, ItemInfo>,
    IRequestHandler<GetHistoryCommand, List<PostInfo>>,
    IRequestHandler<SaveMediaCommand, string>,
    IRequestHandler<CheckArticleCommand, (bool isSuccess, bool isNew, PostInfo? PostInfo)>,
    IRequestHandler<GetAllNews, List<NewsShortContent>>
{
    private readonly IPostRepository _postRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IFeatureRepository _featureRepository;
    private readonly IFileService _fileService;
    private readonly INewsGetterService _newsGetterService;

    public BridgeCommandsHandler(
        IPostRepository postRepository,
        IItemRepository itemRepository,
        IFeatureRepository featureRepository,
        IFileService fileService,
        INewsGetterService newsGetterService)
    {
        _postRepository = postRepository;
        _itemRepository = itemRepository;
        _fileService = fileService;
        _newsGetterService = newsGetterService;
        _featureRepository = featureRepository;
    }

    public async Task<PostInfo> Handle(GetPostCommand request, CancellationToken cancellationToken)
    {
        if (request.ExternalId is not null && request.UseExternalId)
            return await _postRepository.GetAsync(request.ExternalId, cancellationToken) ??
                   throw new ArgumentOutOfRangeException(nameof(request.PostId), "Incorrect external id");

        return await _postRepository.GetAsync(request.PostId, cancellationToken) ??
               throw new ArgumentOutOfRangeException(nameof(request.PostId), "Incorrect post id");
    }

    public Task<long> Handle(CreatePostCommand request, CancellationToken cancellationToken) =>
        _postRepository.CreateAsync(request.AuthorId, request.DataSource, request.ExternalId, cancellationToken);

    public async Task<Unit> Handle(AddItemCommand request, CancellationToken cancellationToken)
    {
        await _itemRepository.AddItemAsync(request.PostId, request.Text, request.FilePath, request.Url, request.Id,
            request.Type,
            cancellationToken);

        return new Unit();
    }

    public async Task<ItemInfo> Handle(GetItemCommand request, CancellationToken cancellationToken) =>
        await _itemRepository.GetAsync(request.ItemId, cancellationToken) ??
        throw new ArgumentOutOfRangeException(nameof(request.ItemId), "Incorrect item id");

    public Task<string> Handle(SaveMediaCommand request, CancellationToken cancellationToken) =>
        _fileService.SaveFileAsync(request.Content, request.FileName, request.FileFormat, cancellationToken);

    public async Task<Unit> Handle(AddFeatureCommand request, CancellationToken cancellationToken)
    {
        await _featureRepository.AddFeatureAsync(Guid.Parse(request.ItemId), request.Type, request.Text,
            cancellationToken);

        return new Unit();
    }

    public Task<List<PostInfo>> Handle(GetHistoryCommand request, CancellationToken cancellationToken) =>
        _postRepository.GetAllAsync(request.AuthorId, request.DataSource, cancellationToken);

    public async Task<(bool isSuccess, bool isNew, PostInfo? PostInfo)> Handle(CheckArticleCommand request,
        CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetAsync(request.Url, cancellationToken);

        if (post is not null)
            return post.DataSource.Equals(request.DataSource, StringComparison.InvariantCultureIgnoreCase)
                ? (isSuccess: true, isNew: false, PostInfo: post)
                : (isSuccess: false, isNew: false, PostInfo: null);


        var (isSuccess, news) = await _newsGetterService.GetNews(request.Url, cancellationToken);

        if (!isSuccess)
            return (isSuccess: false, isNew: false, PostInfo: null);

        post = await _postRepository.GetAsync(news.Url, cancellationToken);

        if (post is not null)
            return post.DataSource.Equals(request.DataSource, StringComparison.InvariantCultureIgnoreCase)
                ? (isSuccess: true, isNew: false, PostInfo: post)
                : (isSuccess: false, isNew: false, PostInfo: null);

        var postId =
            await _postRepository.CreateAsync(news.SourceType, request.DataSource, news.Url, cancellationToken);

        await _itemRepository.AddItemAsync(postId, news.Content, null, null, null, "Text",
            cancellationToken);

        foreach (var imageUrl in news.Images)
            await _itemRepository.AddItemAsync(postId, null, null, imageUrl, null, "ImageUrl",
                cancellationToken);

        var createdPost = await _postRepository.GetAsync(postId, cancellationToken);

        return (isSuccess: true, isNew: true, PostInfo: createdPost);
    }

    public async Task<List<NewsShortContent>> Handle(GetAllNews request, CancellationToken cancellationToken) =>
        await _newsGetterService.GetAllNews(request.Source, request.Page, cancellationToken);
}