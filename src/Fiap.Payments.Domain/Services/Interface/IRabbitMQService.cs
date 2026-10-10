namespace MessageProducer.Services
{
    public interface IRabbitMQService
    {
        Task SendMessageAsync(string message);
    }
}
