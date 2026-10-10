using MessageProducer.Services;

namespace Fiap.Payments.Api.Services.IoC
{
	/// <summary>
	/// Classe de configuração de IoC
	/// </summary>
	public static class Configuration
	{
		/// <summary>
		/// Registrar as dependencias dentro do motor de IoC
		/// </summary>
		/// <param name="services"></param>
		/// <param name="configuration"></param>
		/// <returns></returns>
		public static IServiceCollection RegisterDependencies(this IServiceCollection services, IConfiguration configuration)
		{
      //Interfaces
      services.AddScoped<IRabbitMQService, RabbitMQService>();

      // Domain Services			
      services.AddScoped<Domain.Services.PagamentoService>();

      return services;
		}
	}
}
