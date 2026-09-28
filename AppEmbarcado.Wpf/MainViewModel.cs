using System.ComponentModel;
using System.Runtime.CompilerServices;
using AppEmbarcado.Services;

namespace AppEmbarcado.Wpf
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly RepositorioLeituras repositorio;
        private string dispositivoId = "Sem dados";
        private string capacitivoAdc = "Sem dados";
        private string resistivoAdc = "Sem dados";
        private string temperatura = "Sem dados";
        private string umidade = "Sem dados";
        private string statusBomba = "Sem dados";
        private string ultimaLeitura = "Aguardando primeira leitura";

        public MainViewModel(RepositorioLeituras repositorio)
        {
            this.repositorio = repositorio;
        }

        public string DispositivoId
        {
            get => dispositivoId;
            private set
            {
                if (dispositivoId == value) return;
                dispositivoId = value;
                OnPropertyChanged();
            }
        }

        public string CapacitivoAdc
        {
            get => capacitivoAdc;
            private set
            {
                if (capacitivoAdc == value) return;
                capacitivoAdc = value;
                OnPropertyChanged();
            }
        }

        public string ResistivoAdc
        {
            get => resistivoAdc;
            private set
            {
                if (resistivoAdc == value) return;
                resistivoAdc = value;
                OnPropertyChanged();
            }
        }

        public string Temperatura
        {
            get => temperatura;
            private set
            {
                if (temperatura == value) return;
                temperatura = value;
                OnPropertyChanged();
            }
        }

        public string UmidadeAr
        {
            get => umidade;
            private set
            {
                if (umidade == value) return;
                umidade = value;
                OnPropertyChanged();
            }
        }

        public string StatusBomba
        {
            get => statusBomba;
            private set
            {
                if (statusBomba == value) return;
                statusBomba = value;
                OnPropertyChanged();
            }
        }

        public string UltimaLeitura
        {
            get => ultimaLeitura;
            private set
            {
                if (ultimaLeitura == value) return;
                ultimaLeitura = value;
                OnPropertyChanged();
            }
        }

        public void Atualizar()
        {
            var leitura = repositorio.ObterUltima();
            if (leitura is null)
            {
                UltimaLeitura = "Aguardando primeira leitura";
            }
            else
            {
                var segundos = Math.Max(
                    0,
                    (int)(DateTimeOffset.UtcNow - leitura.RecebidaEmUtc).TotalSeconds);


                UltimaLeitura = segundos > 30
                    ? $"Dados desatualizados há {segundos} s"
                    : $"Dados recebidos há {segundos} s";
            }

            DispositivoId = leitura?.Dados.DispositivoId is string dispositivo
                ? $"{dispositivo}"
                : "Sem dados";
            CapacitivoAdc = leitura?.Dados.CapacitivoAdc is int capacitivo
                ? $"{capacitivo} ADC"
                : "Sem dados";
            ResistivoAdc = leitura?.Dados.ResistivoAdc is int resistivo
                ? $"{resistivo} ADC"
                : "Sem dados";
            Temperatura = leitura?.Dados.TemperaturaC is double temperatura
                ? $"{temperatura} ºC"
                : "Sem dados";
            UmidadeAr = leitura?.Dados.UmidadeArPercentual is double umidade
                ? $"{umidade} %"
                : "Sem dados";
            StatusBomba = leitura?.Dados.BombaLigada switch
            {
                true => "Ligada",
                false => "Desligada",
                null => "Sem dados"
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? nome = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nome));
    }
}