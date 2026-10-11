using Fiap.Payments.Api.Services.Extensions;
using Fiap.Payments.Domain.Services;
using Fiap.Payments.Domain.Services.Event;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Fiap.Payments.Api.Services.RabbitMQ
{
  public class ConsumerOrderPlacedEventService : BackgroundService
  {
    readonly IConfiguration _configuration;
    readonly IServiceProvider _serviceProvider;
    private const string QueueName = "queue-order-placed";
    private IConnection? _connection;
    private IChannel? _channel;

    public ConsumerOrderPlacedEventService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
      _configuration = configuration;
      _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      Console.WriteLine("============ MESSAGE CONSUMER (PAYMENTS) - ORDER PLACED EVENT ============");

      var factory = _configuration.CarregarConnectionFactory();

      Console.WriteLine($"Host: {factory.HostName}:{factory.Port} | User: {factory.UserName}");

      bool flowControl = await TentativasConexao(factory, stoppingToken);

      if (!flowControl) return;

      if (_channel is null) return;

      await _channel.QueueDeclareAsync(
          queue: QueueName,
          durable: true,
          exclusive: false,
          autoDelete: false,
          arguments: null,
          cancellationToken: stoppingToken);

      Console.WriteLine($"Aguardando mensagens da fila '{QueueName}'...");

      var consumer = new AsyncEventingBasicConsumer(_channel);

      consumer.ReceivedAsync += async (model, ea) =>
      {
        var body = ea.Body.ToArray();

        var jsonMessage = Encoding.UTF8.GetString(body);

        var eventResult = JsonSerializer.Deserialize<EventResult<ProcessarPagamentoEvent>>(jsonMessage);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Mensagem recebida: {eventResult?.Event} - {eventResult?.Key}");

        await Task.Delay(1000);

        var msg = eventResult?.ProcessarMensagemConsole();

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {msg}");

        using (var scope = _serviceProvider.CreateScope())
        {
          var pagamentoService = scope.ServiceProvider.GetRequiredService<PagamentoService>();
         await pagamentoService.Processar(eventResult);
        }
      };

      await _channel.BasicConsumeAsync(
        queue: QueueName,
        autoAck: true,
        consumer: consumer,
        cancellationToken: stoppingToken);

      await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task<bool> TentativasConexao(ConnectionFactory factory, CancellationToken stoppingToken)
    {
      const int maxTentativas = 10;
      for (int tentativa = 1; tentativa <= maxTentativas && !stoppingToken.IsCancellationRequested; tentativa++)
      {
        try
        {
          _connection = await factory.CreateConnectionAsync(stoppingToken);
          _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);
          break;
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Tentativa {tentativa}/{maxTentativas} falhou: {ex.Message}");
          if (tentativa == maxTentativas) return false;
          await Task.Delay(3000, stoppingToken);
        }
      }

      return true;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
      if (_channel is not null) await _channel.CloseAsync(cancellationToken);
      if (_connection is not null) await _connection.CloseAsync(cancellationToken);
      await base.StopAsync(cancellationToken);
    }
  }
}