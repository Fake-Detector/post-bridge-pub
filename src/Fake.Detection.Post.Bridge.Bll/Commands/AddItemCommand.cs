using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record AddItemCommand(
    long PostId,
    string Type,
    string? Text = null,
    string? FilePath = null,
    string? Url = null,
    Guid? Id = null) : IRequest<Unit>;