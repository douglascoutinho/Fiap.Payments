using MessageProducer.Services;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class PublishService : IAsyncDisposable, IRabbitMQService
{
  private readonly IConfiguration _configuration;
  readonly ConnectionFactory _factory;
  private IConnection? _connection;

  public PublishService(IConfiguration configuration)
  {
    _configuration = configuration;

    _factory = new ConnectionFactory()
    {
      HostName = _configuration["RabbitMQ:HostName"] ?? string.Empty,
      UserName = _configuration["RabbitMQ:UserName"] ?? string.Empty,
      Password = _configuration["RabbitMQ:Password"] ?? string.Empty,
      AutomaticRecoveryEnabled = true
    };
  }

  private async Task<IConnection> GetConnectionAsync()
  {
    if (_connection == null || !_connection.IsOpen)
    {
      _connection = await _factory.CreateConnectionAsync();
    }
    return _connection;
  }

  public async Task SendMessageAsync<T>(T messageObject, string targetQueue, string exchangeName = "")
  {
    var connection = await GetConnectionAsync();

    await using var channel = await connection.CreateChannelAsync();

    await channel.QueueDeclareAsync
    (
        queue: targetQueue,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null
    );

    string jsonString = JsonSerializer.Serialize(messageObject);
    var messageBody = Encoding.UTF8.GetBytes(jsonString);

    var properties = new BasicProperties
    {
      ContentType = "application/json",
      DeliveryMode = DeliveryModes.Persistent
    };

    await channel.BasicPublishAsync
    (
        exchange: exchangeName,
        routingKey: targetQueue,
        mandatory: false,
        basicProperties: properties,
        body: messageBody
    );
  }

  public async ValueTask DisposeAsync()
  {
    if (_connection != null)
    {
      await _connection.CloseAsync();
      _connection.Dispose();
    }
  }
}