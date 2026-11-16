using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using AppEmbarcado.Services;

namespace AppEmbarcado.Wpf
{
    public partial class MainWindow : Window
    {
        private readonly RepositorioLeituras repositorio = new();
        private readonly ClienteMqtt mqtt;
        private readonly MainViewModel viewModel;
        private readonly DispatcherTimer timer;
        private bool mqttIniciado;
        private bool encerrando;

        public MainWindow()
        {
            InitializeComponent();

            var dbPath = Path.Combine(FindSolutionRoot(), "dados", "leituras.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            var armazenamento = new ArmazenamentoSqlite(dbPath);

            mqtt = new ClienteMqtt(
                repositorio,
                armazenamento,
                host: "localhost",
                porta: 1883,
                usuario: "appcsharp",
                senha: "1875"
            );
            mqtt.Log += msg => Debug.WriteLine(msg);

            viewModel = new MainViewModel(repositorio);
            DataContext = viewModel;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (_, _) => viewModel.Atualizar();

            Loaded += JanelaCarregada;
            Closing += JanelaFechando;
        }
        private static string FindSolutionRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir is not null && dir.GetFiles("*.sln*").Length == 0)
                dir = dir.Parent;
            return dir?.FullName ?? AppContext.BaseDirectory;
        }

        private async void JanelaCarregada(object sender, RoutedEventArgs e)
        {
            try
            {
                await mqtt.IniciarAsync();
                mqttIniciado = true;
                viewModel.Atualizar();
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível iniciar o cliente MQTT: {ex.Message}");
                Close();
            }
        }

        private async void JanelaFechando(object? sender, CancelEventArgs e)
        {
            if (encerrando) return;

            e.Cancel = true;
            encerrando = true;
            timer.Stop();

            try
            {
                if (mqttIniciado)
                {
                    await mqtt.PararAsync();
                }
            }
            finally
            {
                await mqtt.DisposeAsync();
                Close();
            }
        }
    }
}