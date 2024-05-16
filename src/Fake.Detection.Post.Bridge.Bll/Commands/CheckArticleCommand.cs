using Fake.Detection.Post.Bridge.Bll.Models;
using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record CheckArticleCommand(string Url, string Source, string DataSource)
    : IRequest<(bool isSuccess, bool isNew, PostInfo? PostInfo)>;