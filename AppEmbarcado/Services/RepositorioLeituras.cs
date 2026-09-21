using AppEmbarcado.Models;

namespace AppEmbarcado.Services;

/// <summary>Armazena somente a última mensagem em memória; os dados se perdem ao fechar o programa.</summary>
public sealed class RepositorioLeituras
{
    private readonly object sincronizacao = new();
    private LeituraRecebida? ultima;

    public LeituraRecebida? ObterUltima()
    {
        lock (sincronizacao)
            return ultima;
    }

    public LeituraRecebida Registrar(LeituraSensores dados)
    {
        if (ValidadorLeitura.Validar(dados).Count > 0)
            throw new ArgumentException("A leitura contém dados inválidos.", nameof(dados));

        lock (sincronizacao)
        {
            ultima = new LeituraRecebida(dados, DateTimeOffset.UtcNow);
            return ultima;
        }
    }
}
