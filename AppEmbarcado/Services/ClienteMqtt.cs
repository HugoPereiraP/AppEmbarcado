using System.Text.Json;
using AppEmbarcado.Models;
using MQTTnet;
using MQTTnet.Client;

namespace AppEmbarcado.Services;

/// <summary>Assina o tópico de leituras no Mosquitto e grava as válidas no repositório.</summary>
public sealed class ClienteMqtt : IAsyncDisposable
{
    public const string TopicoPadrao = "appembarcado/leituras";

    private static readonly JsonSerializerOptions JsonOpcoes = new(JsonSerializerDefaults.Web);

    private readonly RepositorioLeituras repositorio;
    private readonly IMqttClient cliente;
    private readonly MqttClientOptions opcoes;
    private readonly MqttClientSubscribeOptions assinatura;
    private CancellationTokenSource? cts;
    private Task? tarefaConexao;

    public event Action<string>? Log;

    public ClienteMqtt(
        RepositorioLeituras repositorio,
        string host,
        int porta,
        string usuario,
        string senha,
        string topico = TopicoPadrao)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        if (porta is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(porta));

        this.repositorio = repositorio;

        opcoes = new MqttClientOptionsBuilder()
            .WithTcpServer(host, porta)
            .WithClientId($"wpf-app-{Guid.NewGuid():N}")
            .WithCredentials(usuario, senha)
            .WithCleanSession()
            .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
            .Build();

        var factory = new MqttFactory();
        assinatura = factory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(f => f.WithTopic(topico).WithAtLeastOnceQoS())
            .Build();

        cliente = factory.CreateMqttClient();
        cliente.ApplicationMessageReceivedAsync += AoReceberMensagem;
    }

    /// <summary>Não lança se o broker estiver fora do ar: o laço continua tentando.</summary>
    public Task IniciarAsync()
    {
        cts = new CancellationTokenSource();
        tarefaConexao = MantarConexaoAsync(cts.Token);
        return Task.CompletedTask;
    }

    private async Task MantarConexaoAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (!cliente.IsConnected)
            {
                try
                {
                    await cliente.ConnectAsync(opcoes, ct);
                    // Sessão limpa: é preciso assinar de novo a cada reconexão.
                    await cliente.SubscribeAsync(assinatura, ct);
                    Log?.Invoke("MQTT conectado e assinando o tópico.");
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Log?.Invoke($"Falha MQTT: {ex.Message}. Nova tentativa em 5 s.");
                    if (cliente.IsConnected)
                        await cliente.DisconnectAsync(); // conectou mas não assinou: recomeça
                }
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private Task AoReceberMensagem(MqttApplicationMessageReceivedEventArgs e)
    {
        try
        {
            var json = e.ApplicationMessage.ConvertPayloadToString();
            var leitura = JsonSerializer.Deserialize<LeituraSensores>(json, JsonOpcoes);

            if (leitura is null)
            {
                Log?.Invoke("Mensagem vazia ignorada.");
                return Task.CompletedTask;
            }

            var erros = ValidadorLeitura.Validar(leitura);
            if (erros.Count > 0)
            {
                var resumo = string.Join("; ", erros.Select(x => $"{x.Key}: {string.Join(", ", x.Value)}"));
                Log?.Invoke($"Leitura inválida descartada: {resumo}");
                return Task.CompletedTask;
            }

            repositorio.Registrar(leitura);
        }
        catch (JsonException ex)
        {
            Log?.Invoke($"JSON inválido descartado: {ex.Message}");
        }
        catch (Exception ex)
        {
            Log?.Invoke($"Erro ao processar mensagem: {ex.Message}");
        }

        // Nunca propaga exceção: uma mensagem ruim não derruba as próximas.
        return Task.CompletedTask;
    }

    public async Task PararAsync()
    {
        if (cts is null) return;

        cts.Cancel();
        if (tarefaConexao is not null)
            await tarefaConexao;

        if (cliente.IsConnected)
            await cliente.DisconnectAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await PararAsync();
        cliente.Dispose();
        cts?.Dispose();
    }
}