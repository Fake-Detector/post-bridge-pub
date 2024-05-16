using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record CreatePostCommand(string AuthorId, string DataSource, string? ExternalId) : IRequest<long>;