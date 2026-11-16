namespace AppEmbarcado.Models
{
    public sealed record PontoHistorico(
        DateTimeOffset Instante,
        double? CapacitivoAdc,
        double? ResistivoAdc,
        double? TemperaturaC,
        double? UmidadeAr);
}
