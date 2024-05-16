using Fake.Detection.Post.Bridge.Bll.Models;
using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record GetItemCommand(Guid ItemId) : IRequest<ItemInfo>;