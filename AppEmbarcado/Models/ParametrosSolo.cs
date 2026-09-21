namespace AppEmbarcado.Models;

/// <summary>
/// Referências experimentais em ADC para um sensor e solo específicos.
/// Não representam porcentagem de umidade nem ativam automaticamente a bomba.
/// A ordem dos valores depende da resposta de cada sensor.
/// </summary>
public sealed record ReferenciasSoloAdc
{
    public int? CapacidadeDeCampoAdc { get; init; }
    public int? PontoDeMurchaAdc { get; init; }
    public bool Completa => CapacidadeDeCampoAdc.HasValue && PontoDeMurchaAdc.HasValue;
}

public sealed record ParametrosSolo
{
    public ReferenciasSoloAdc Capacitivo { get; init; } = new();
    public ReferenciasSoloAdc Resistivo { get; init; } = new();
}
