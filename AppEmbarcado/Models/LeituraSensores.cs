namespace AppEmbarcado.Models;

/// <summary>Mensagem enviada pelo ESP32. Null significa dado ainda não disponível.</summary>
public sealed record LeituraSensores
{
    public string? DispositivoId { get; init; }
    public int? CapacitivoAdc { get; init; }
    public int? ResistivoAdc { get; init; }
    public double? TemperaturaC { get; init; }
    public double? UmidadeArPercentual { get; init; }
    public bool? BombaLigada { get; init; }
}
