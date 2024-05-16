using Fake.Detection.Post.Bridge.Api.Extensions;
using Fake.Detection.Post.Bridge.Bll.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fake.Detection.Post.Bridge.Api.Controllers;

[ApiController]
[Route("[controller]/data")]
public class FileController : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFile(Guid id, [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var item = await mediator.Send(new GetItemCommand(id), cancellationToken);

        if (string.IsNullOrWhiteSpace(item.FilePath))
            throw new ArgumentNullException(nameof(item.FilePath), "FilePath is null");

        var fileStream = new FileStream(item.FilePath!, FileMode.Open, FileAccess.Read);

        var fileName = Path.GetFileName(item.FilePath);

        return File(fileStream, item.FilePath.GetMimeType(), fileName);
    }

    [HttpGet("url/{id:guid}")]
    public Task<string?> GetUrl(Guid id)
    {
        return Task.FromResult(Url.Action("GetFile", "File", new { id }, Request.Scheme));
    }
}