using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.Catalog.Api.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "Administrador,Usuario")]
  public class AmbienteController : ControllerBase
  {
    readonly IConfiguration _configuration;

    public AmbienteController(IConfiguration configuration)
    {
      _configuration = configuration;
    }

    /// <summary>
    /// Verrificar se as variáveis de ambiente do RabbitMQ etao sendo carregadas corretamente.
    /// </summary>
    /// <returns></returns>
    /// <response code="200"></response>
    [HttpGet("Variavel-Secret-V1")]
    [ProducesResponseType(typeof(string), 200)]
    [AllowAnonymous]
    public IActionResult Variavel()
    {
      var variaveis = new
      {
        hosterName = _configuration["RabbitMQ:Host"],
        port = _configuration["RabbitMQ:Port"],
        userName = _configuration["RabbitMQ:User"],
        password = _configuration["RabbitMQ:Password"],
        virtualHost = _configuration["RabbitMQ:VirtualHost"]
    };

      return Ok(variaveis);
 
    }
  }
}