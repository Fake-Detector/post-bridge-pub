using Fake.Detection.Post.Bridge.Bll.Models;
using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record GetHistoryCommand(string AuthorId, string DataSource) : IRequest<List<PostInfo>>;