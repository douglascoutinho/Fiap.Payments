using Fiap.Users.Domain.Entity.Enum;

namespace Fiap.Payments.Domain.Services.Event
{
  public class ProcessarPagamentoEvent
  {
    public int IdUsuario { get; set; }
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public int IdJogo { get; set; }
    public decimal Preco { get; set; }
    public StatusPagamentoEnum StatusPagamento { get; set; }
  }
}
