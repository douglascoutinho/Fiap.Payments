namespace Fiap.Payments.Domain.Services.Event
{
  /// <summary>
  ///  Event Result
  /// </summary>
  public abstract class EventResult
  {
    /// <summary>
    /// Identificador
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    /// Nome Evento
    /// </summary>
    public string? Event { get; set; }
  }

  /// <summary>
  /// Event Result
  /// </summary>
  public class EventResult<T> : EventResult
  {
    /// <summary>
    /// Data
    /// </summary>
    public T? Data { get; set; }
  }
}
