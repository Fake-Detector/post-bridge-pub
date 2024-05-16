using Fake.Detection.Post.Bridge.Bll.Models;
using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record GetPostCommand(long PostId, string? ExternalId = null, bool UseExternalId = false) : IRequest<PostInfo>;