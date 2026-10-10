using Fiap.Payments.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fiap.Payments.Api.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  [Authorize(Roles = "Administrador,Usuario")]
  public class PagamentoController : ControllerBase
  {
    readonly PagamentoService _pagamentoService;

    public PagamentoController(PagamentoService pagamentoService)
    {
      _pagamentoService = pagamentoService;
    }

    /// <summary>
    /// Processar um Pagamento de um Jogo adicionado a Biblioteca
    /// </summary>
    /// <param name="dto">Corpo da requisição</param>
    /// <returns></returns>
    /// <response code="200"></response>
    [HttpPost("Processar")]
    [ProducesResponseType(typeof(string), 200)]
    public IActionResult Post()
    {
      _pagamentoService.Processar();

      return Ok("Pagamento Processado Com Sucesso.");
    }
  }
}