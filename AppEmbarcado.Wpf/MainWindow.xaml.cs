using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using AppEmbarcado.Services;

namespace AppEmbarcado.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly RepositorioLeituras repositorio = new();
        private readonly ServidorHttp servidor;
        private readonly MainViewModel viewModel;
        private readonly DispatcherTimer timer;
        private bool servidorIniciado;
        private bool encerrando;

        public MainWindow()
        {
            InitializeComponent();

            servidor = new ServidorHttp(repositorio);
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

        private async void JanelaCarregada(object sender, RoutedEventArgs e)
        {
            try
            {
                await servidor.IniciarAsync();
                servidorIniciado = true;
                viewModel.Atualizar();
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível iniciar o servidor: {ex.Message}");
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
                if (servidorIniciado)
                {
                    await servidor.PararAsync();
                }
            }
            finally
            {
                await servidor.DisposeAsync();
                Close();
            }
        }
    }
}