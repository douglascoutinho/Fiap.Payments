using Fiap.Payments.Domain.Entity;
using Fiap.Payments.Domain.Services.Event;
using MessageProducer.Services;
using System.Diagnostics;

namespace Fiap.Payments.Domain.Services
{
  public class PagamentoService
  {
    IRabbitMQService _rabbitMQ;

    public PagamentoService(IRabbitMQService rabbitMQ)
    {
      _rabbitMQ = rabbitMQ;
    }

    public async Task Processar(EventResult<ProcessarPagamentoEvent> result)
    {
      var input = new EventResult<ProcessarPagamentoEvent>
      {
        Key = result.Key,
        Event = "PaymentProcessedEvent",
        Data = new ProcessarPagamentoEvent
        {
          IdUsuario = result?.Data?.IdUsuario ?? 0,
          IdJogo = result?.Data?.IdJogo ?? 0,
          Preco = result?.Data?.Preco ?? 0,
          StatusPagamento = SimuladorPagamento.Simular()
        }
      };

      await _rabbitMQ.SendMessageAsync(input, "payment-processed-event", "payment-processed-exchange");
    }
  }
}
