using Fiap.Payments.Domain.Services.Event;
using RabbitMQ.Client;

namespace Fiap.Payments.Api.Services.Extensions
{
  public static class Extension
  {
    public static ConnectionFactory CarregarConnectionFactory(this IConfiguration config)
    {
      var factory = new ConnectionFactory
      {
        HostName = config["RabbitMQ:HostName"] ?? string.Empty,
        Port = int.Parse(config["RabbitMQ:Port"] ?? string.Empty),
        UserName = config["RabbitMQ:UserName"] ?? string.Empty,
        Password = config["RabbitMQ:Password"] ?? string.Empty,
        VirtualHost = config["RabbitMQ:VirtualHost"] ?? "/"
      };

      return factory;
    }

    public static string ProcessarMensagemConsole(this EventResult<ProcessarPagamentoEvent> result)
    {
      return $"Pagamento Enviado Para Analise: UserId {result?.Data?.IdUsuario} - GameId : {result?.Data?.IdJogo} - Price: {result?.Data?.Preco} - {result?.Key}";
    }
  }
}

