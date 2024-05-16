using MediatR;

namespace Fake.Detection.Post.Bridge.Bll.Commands;

public record SaveMediaCommand(byte[] Content, string FileName, string FileFormat) : IRequest<string>;