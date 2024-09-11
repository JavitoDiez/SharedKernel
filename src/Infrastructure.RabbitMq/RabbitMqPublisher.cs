﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System.Text;

namespace SharedKernel.Infrastructure.RabbitMq;

/// <summary> . </summary>
public class RabbitMqPublisher
{
    private const string HeaderReDelivery = "redelivery_count";
    private readonly ILogger<RabbitMqPublisher> _logger;
    private readonly RabbitMqConnectionFactory _config;
    private readonly IOptions<RabbitMqConfigParams> _rabbitMqParams;

    /// <summary> . </summary>
    public RabbitMqPublisher(
        ILogger<RabbitMqPublisher> logger,
        RabbitMqConnectionFactory config,
        IOptions<RabbitMqConfigParams> rabbitMqParams)
    {
        _logger = logger;
        _config = config;
        _rabbitMqParams = rabbitMqParams;
    }

    /// <summary> </summary>
    public Task PublishTopic(string textMessage, string topicName)
    {
        return PublishCommon(textMessage, true, topicName);
    }

    /// <summary> </summary>
    public Task PublishOnQueue(string textMessage, string queue)
    {
        return PublishCommon(textMessage, false, queue);
    }

    private Task PublishCommon(string textMessage, bool isTopic, string name)
    {
        try
        {
            var queue = isTopic ? _rabbitMqParams.Value.ExchangeName : _rabbitMqParams.Value.PublishQueue;
            var exchangeType = isTopic ? ExchangeType.Topic : ExchangeType.Direct;

            var channel = _config.Channel();
            channel.ExchangeDeclare(queue, exchangeType);

            var body = Encoding.UTF8.GetBytes(textMessage);
            var properties = channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, object> { { HeaderReDelivery, 0 } };

            channel.BasicPublish(queue, name, properties, body);
        }
        catch (RabbitMQClientException ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return Task.CompletedTask;
    }
}
