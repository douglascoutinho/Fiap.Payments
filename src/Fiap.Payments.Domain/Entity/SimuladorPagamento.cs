using Fiap.Users.Domain.Entity.Enum;

namespace Fiap.Payments.Domain.Entity
{
  public class SimuladorPagamento
  {
    private static readonly Random _random = new Random();

    public static StatusPagamentoEnum Simular()
    {
      // Gera um número entre 1 e 100
      int chance = _random.Next(1, 101);

      // 80% de chance de ser Aprovado, 20% de Reprovado
      return chance <= 80
          ? StatusPagamentoEnum.Aprovado
          : StatusPagamentoEnum.Reprovado;
    }
  }
}
