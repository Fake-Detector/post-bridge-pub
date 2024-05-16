using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record AddFeatureCommand(string ItemId, string Type, string Text) : IRequest<Unit>;