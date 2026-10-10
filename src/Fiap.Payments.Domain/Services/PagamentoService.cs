using MessageProducer.Services;

namespace Fiap.Payments.Domain.Services
{
  public class PagamentoService
  {
    IRabbitMQService _rabbitMQ;

    public PagamentoService(IRabbitMQService rabbitMQ)
    {
      _rabbitMQ = rabbitMQ;
    }

    public void Processar()
    {
      _rabbitMQ.SendMessageAsync("Pagamento Processado Com Sucesso");
    }
  }
}
