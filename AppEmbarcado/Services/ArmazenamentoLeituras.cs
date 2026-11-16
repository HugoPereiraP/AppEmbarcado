using AppEmbarcado.Models;

namespace AppEmbarcado.Services
{
    public interface IArmazenamentoLeituras
    {
        void Salvar(DateTimeOffset recebidaEmUtc, LeituraSensores dados);
        IReadOnlyList<PontoHistorico> Consultar(string dispositivoId, DateTimeOffset de, DateTimeOffset ate, int janelaSegundos);
    }
}
