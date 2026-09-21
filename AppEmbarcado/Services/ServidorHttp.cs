using AppEmbarcado.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AppEmbarcado.Services;

/// <summary>Receptor HTTP para a rede local. Inicie e descarte junto com a futura aplicação WPF.</summary>
public sealed class ServidorHttp : IAsyncDisposable
{
    private readonly WebApplication aplicacao;

    public ServidorHttp(RepositorioLeituras repositorio, int porta = 5000)
    {
        ArgumentNullException.ThrowIfNull(repositorio);
        if (porta is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(porta));

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://0.0.0.0:{porta}");
        aplicacao = builder.Build();

        aplicacao.MapGet("/api/status", () => Results.Ok(new { status = "online" }));

        aplicacao.MapGet("/api/leituras/ultima", () =>
        {
            var leitura = repositorio.ObterUltima();
            return leitura is null ? Results.NoContent() : Results.Ok(leitura);
        });

        aplicacao.MapPost("/api/leituras", (LeituraSensores dados) =>
        {
            var erros = ValidadorLeitura.Validar(dados);
            if (erros.Count > 0)
                return Results.ValidationProblem(erros);

            return Results.Ok(repositorio.Registrar(dados));
        });
    }

    public Task IniciarAsync(CancellationToken cancellationToken = default) =>
        aplicacao.StartAsync(cancellationToken);

    public Task PararAsync(CancellationToken cancellationToken = default) =>
        aplicacao.StopAsync(cancellationToken);

    public ValueTask DisposeAsync() => aplicacao.DisposeAsync();
}
