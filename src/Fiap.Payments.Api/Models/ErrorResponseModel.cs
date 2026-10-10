namespace Fiap.Payments.Api.Models
{
  /// <summary>
  /// Modelo de dados para retorno da API
  /// </summary>
  public class ErrorResponseModel
  {

    /// <summary>
    /// Status HTTP do erro(4xx, 5xx, etc..)
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// lista com as mensagens de erro
    /// </summary>
    public List<String>? Errors { get; set; }

  }
}
