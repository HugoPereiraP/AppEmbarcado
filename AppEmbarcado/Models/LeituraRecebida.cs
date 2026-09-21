namespace AppEmbarcado.Models;

/// <summary>O horário é registrado pelo computador, sem exigir relógio sincronizado no ESP32.</summary>
public sealed record LeituraRecebida(LeituraSensores Dados, DateTimeOffset RecebidaEmUtc);
