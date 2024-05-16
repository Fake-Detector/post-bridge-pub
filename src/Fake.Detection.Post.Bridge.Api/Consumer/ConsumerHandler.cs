using Common.Library.Kafka.Consumer.Interfaces;
using Confluent.Kafka;
using Fake.Detection.Post.Bridge.Bll.Commands;
using Fake.Detection.Post.Bridge.Contracts;
using Fake.Detection.Post.Monitoring.Client.Services.Interfaces;
using Fake.Detection.Post.Monitoring.Messages;
using MediatR;
using Newtonsoft.Json;

namespace Fake.Detection.Post.Bridge.Api.Consumer;

public class ConsumerHandler : IConsumerHandler<Feature>
{
    private readonly IMediator _mediator;
    private readonly IMonitoringClient _monitoringClient;
    private readonly ILogger<ConsumerHandler> _logger;

    public ConsumerHandler(
        IMediator mediator,
        IMonitoringClient monitoringClient,
        ILogger<ConsumerHandler> logger)
    {
        _mediator = mediator;
        _monitoringClient = monitoringClient;
        _logger = logger;
    }

    public async Task HandleMessage(ConsumeResult<string, Feature> message, CancellationToken cancellationToken)
    {
        var feature = message.Message.Value;

        try
        {
            await _mediator.Send(new AddFeatureCommand(feature.ItemId, feature.Type.ToString(), feature.Text),
                cancellationToken);
        }
        catch (Exception e)
        {
            await _monitoringClient.SendToMonitoring(
                MonitoringType.Error,
                JsonConvert.SerializeObject(feature),
                JsonConvert.SerializeObject(e));

            _logger.LogError(e, "Error while handling: {Message}", JsonConvert.SerializeObject(feature));
        }
    }
}