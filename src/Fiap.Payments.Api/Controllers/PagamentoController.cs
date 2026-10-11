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
  }
}