# AppEmbarcado — Monitoramento de solo com ESP32 e C#

Base de classes em .NET 8, preparada para uma futura interface WPF. Ainda não há janela ou executável: este projeto compila como biblioteca.

## Classes

- `Models/LeituraSensores.cs`: contrato JSON com os dois sensores de solo, DHT22 e estado da bomba.
- `Models/LeituraRecebida.cs`: dados recebidos e horário UTC registrado no computador.
- `Models/ParametrosSolo.cs`: referências ADC independentes para cada sensor, inicialmente sem valores. Preencher somente após o procedimento experimental; não calcula umidade percentual nem define regras de irrigação.
- `Services/ValidadorLeitura.cs`: valida identificação, ADC não negativo, temperatura finita e umidade do ar entre 0 e 100.
- `Services/RepositorioLeituras.cs`: última mensagem em memória, com acesso sincronizado para HTTP e interface.
- `Services/ServidorHttp.cs`: receptor ASP.NET Core para a rede local, porta padrão 5000.

## Mensagem inicial do ESP32

Enviar `POST /api/leituras` com `Content-Type: application/json`:

```json
{
  "dispositivoId": "esp32-01",
  "capacitivoAdc": 546,
  "resistivoAdc": 2526
}
```

Os números acima são apenas um exemplo de leitura da foto, não limites de calibração.
Quando disponíveis, adicionar `temperaturaC`, `umidadeArPercentual` e `bombaLigada`.
Campos ausentes ou `null` significam “sem dado”; `bombaLigada: false` significa bomba desligada. Zero é uma leitura válida e não significa ausência.
Cada mensagem substitui a anterior por inteiro: envie todos os dados atualmente disponíveis em cada transmissão.

## Uso na futura aplicação

```csharp
using AppEmbarcado.Services;

var repositorio = new RepositorioLeituras();
await using var servidor = new ServidorHttp(repositorio);
await servidor.IniciarAsync();

// Manter essas instâncias durante a vida da janela.
// A interface pode consultar repositorio.ObterUltima() com DispatcherTimer.
// Ao fechar a aplicação:
await servidor.PararAsync();
```

O trecho demonstra o ciclo de vida; na aplicação real, o encerramento só ocorre quando a janela fechar.
O ESP32 usará `http://IP_DO_COMPUTADOR:5000/api/leituras`, com comunicação permitida entre os dispositivos e a porta liberada no firewall. Não usar `localhost` no ESP32 para acessar o computador. O receptor não tem autenticação e foi destinado à rede local do experimento.

Rotas:

- `GET /api/status`: confirma que o receptor está ativo.
- `POST /api/leituras`: retorna 200 com a leitura registrada; dados inválidos retornam 400.
- `GET /api/leituras/ultima`: retorna 200 com a última leitura ou 204 se ainda não recebeu nenhuma.

## Próximas etapas

1. Criar a janela WPF e conectá-la ao repositório e ao ciclo de vida do servidor.
2. Enviar o JSON pelo ESP32 via Wi-Fi.
3. Adicionar DHT22 e LED ao firmware, enviando os campos opcionais.
4. Definir o procedimento de calibração, registrar as referências e implementar histórico e regras de irrigação.

Compilar: `dotnet build AppEmbarcado.sln`.
