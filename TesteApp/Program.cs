using AppEmbarcado.Services;

var repositorio = new RepositorioLeituras();

await using var servidor = new ServidorHttp(repositorio);
await servidor.IniciarAsync();

Console.WriteLine("Servidor iniciado em http://localhost:5000");
Console.WriteLine("Pressione ENTER para encerrar.");
Console.ReadLine();

await servidor.PararAsync();