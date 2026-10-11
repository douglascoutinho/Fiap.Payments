namespace MessageProducer.Services
{
  public interface IRabbitMQService
  {
    Task SendMessageAsync<T>(T messageObject, string targetQueue, string exchangeName = "");
  }
}
  