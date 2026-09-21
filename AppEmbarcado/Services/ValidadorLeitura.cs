using AppEmbarcado.Models;

namespace AppEmbarcado.Services;

public static class ValidadorLeitura
{
    public static Dictionary<string, string[]> Validar(LeituraSensores leitura)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(leitura.DispositivoId))
            erros["dispositivoId"] = ["Informe o identificador do ESP32."];

        // Não fixamos o máximo do ADC: ele depende da resolução configurada no firmware.
        if (leitura.CapacitivoAdc is < 0)
            erros["capacitivoAdc"] = ["A leitura ADC não pode ser negativa."];
        if (leitura.ResistivoAdc is < 0)
            erros["resistivoAdc"] = ["A leitura ADC não pode ser negativa."];
        if (leitura.TemperaturaC is double temperatura && !double.IsFinite(temperatura))
            erros["temperaturaC"] = ["Informe uma temperatura finita ou null."];
        if (leitura.UmidadeArPercentual is double umidade &&
            (!double.IsFinite(umidade) || umidade < 0 || umidade > 100))
            erros["umidadeArPercentual"] = ["Informe um valor de 0 a 100 ou null."];

        if (leitura.CapacitivoAdc is null && leitura.ResistivoAdc is null &&
            leitura.TemperaturaC is null && leitura.UmidadeArPercentual is null &&
            leitura.BombaLigada is null)
            erros["dados"] = ["Envie pelo menos uma leitura ou o estado da bomba."];

        return erros;
    }
}
