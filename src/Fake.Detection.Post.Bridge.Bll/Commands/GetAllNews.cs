using Fake.Detection.Post.Bridge.Bll.Models;
using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record GetAllNews(string Source, long Page) : IRequest<List<NewsShortContent>>;