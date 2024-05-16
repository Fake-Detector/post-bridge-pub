// Путь к файлу

using Fake.Detection.Post.Bridge.Api;
using Fake.Detection.Post.Bridge.Contracts;
using Grpc.Core;
using Grpc.Net.Client;

var filePath = @"D:\cat_ussr_gpw — копия.png";

using var channel = GrpcChannel.ForAddress("https://localhost:7136");
var client = new PostBridgeService.PostBridgeServiceClient(channel);

var metadata = new Metadata
{
    { "Authorization", "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJsb2dpbiI6ImFkbWluMSIsIm5iZiI6MTcxMjMwNTYzNiwiZXhwIjoxNzEyMzA5MjM2LCJpYXQiOjE3MTIzMDU2MzYsImlzcyI6ImZha2UtZGV0LWF1dGgiLCJhdWQiOiJmYWtlLWRldCJ9.JfuVS0Z0yRyzcSW_B4wT1nPGoy5-T96KK9eSV6SdGrA" }
};

using var call = client.SendPostItem(metadata);

await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
var buffer = new byte[4096];
int bytesRead;
while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
{
    await call.RequestStream.WriteAsync(new SendPostItemRequest
    {
        Item = new ItemChunk
        {
            PostId = 2,
            MetaData = new MetaData
            {
                Type = ItemType.Image,
                Format = "png"
            },
            Chunk = Google.Protobuf.ByteString.CopyFrom(buffer, 0, bytesRead)
        }
    });
}


await call.RequestStream.CompleteAsync();

var response = await call.ResponseAsync;
Console.WriteLine($"Upload status: {response.Result}");