using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
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


namespace TransformaEspectrometroWPF.Forms
{ 
    public partial class MainWindow : Window
    {
        private INI ini = null!;
        private string caminhoCarregado = "";
        private string pastaMatriz = "";
        private string arquivosPendentes = "";
        private string naoProcessado = "";
        private string processado = "";

        public MainWindow()
        {
            InitializeComponent();
        Loaded += MainWindow_Loaded;

        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ini = new INI();

            // Opcional: ler algum valor do INI
            string pastaMatriz = ini.Read("Pasta Matriz", "CAMINHO");
            string arquivosPendentes = ini.Read("Arquivos Pendentes", "CAMINHO");

            await F_TestarComunicacaoAsync();

        }

        private void BtnCarregaArquivo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Selecione um arquivo";
            ofd.Filter = "Arquivo de texto|*.txt";
            ofd.FilterIndex = 1;

            if (ofd.ShowDialog() == true)
            {
                caminhoCarregado = ofd.FileName;
                TbCaminhoArquivo.Text = caminhoCarregado;
                TbCaminhoArquivo.Visibility = Visibility.Visible;
                F_ValidarArquivo();

            }
        }
        private async void BtnTestaComunicacao_Click(object sender, RoutedEventArgs e)
        { 
            pbTestaComunicacoes.Visibility = Visibility.Visible;

            await F_TestarComunicacaoAsync();
        }

        private void BtnMainStartStop_Click(object sender, RoutedEventArgs e)
        {

        }
        private void BtnProcessaManual_Click(object sender, RoutedEventArgs e)
        {

        }

        private void F_HabilitaTodosBotoes(bool isEnabled)
        {
            void DesabilitaRecursivo(DependencyObject parent)
            {
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int i = 0; i < count; i++)
                {
                    var child = VisualTreeHelper.GetChild(parent, i);

                    if (child is Button btn)
                        btn.IsEnabled = isEnabled;

                    DesabilitaRecursivo(child);
                }
            }

            DesabilitaRecursivo(this);

        }



        private bool F_ValidarArquivo()
        {
            try
            {
                if (!string.IsNullOrEmpty(caminhoCarregado))
                {
                    string[] linhas = File.ReadAllLines(caminhoCarregado);
                    string[] cabecalho = linhas[0].Split(';');

                    if (linhas.Length > 2 || linhas.Length == 0 || !cabecalho[0].Contains("Timestamp"))
                    {
                        TbStatusArquivo.Visibility = Visibility.Visible;
                        TbStatusArquivo.Text = "Erro: Arquivo não compatível com o padrão do Espectrômetro";
                        TbStatusArquivo.Foreground = Brushes.Red;
                        BtnProcessaManual.IsEnabled = false;
                        return false;
                    }
                    else
                    {
                        TbStatusArquivo.Visibility = Visibility.Visible;
                        TbStatusArquivo.Text = "Arquivo Válido!";
                        TbStatusArquivo.Foreground = Brushes.Green;
                        BtnProcessaManual.IsEnabled = true;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                TbStatusArquivo.Visibility = Visibility.Visible;
                TbStatusArquivo.Text = "Erro ao ler o arquivo";
                TbStatusArquivo.Foreground = Brushes.Red;
                MessageBox.Show("Erro: " + ex.Message);
                return false;
            }
        }

        private void F_HabilitaTab(bool isEnabled)
        {
            foreach (TabItem tab in MainTabControl.Items)
            {
                tab.IsEnabled = isEnabled;
            }
        }
        private async Task F_TestarComunicacaoAsync()
        {

            F_HabilitaTab(false);
            F_HabilitaTodosBotoes(false);

            // Mostra labels
            TbPastaMatrizError.Visibility = Visibility.Visible;
            TbArquivoNaoProcessadoError.Visibility = Visibility.Visible;
            TbArquivoPendenteError.Visibility = Visibility.Visible;

            // Cria objeto INI
            ini = new INI();

            // Lê caminhos do INI
            pastaMatriz = ini.Read("Pasta Matriz", "CAMINHO");
            arquivosPendentes = ini.Read("Arquivos Pendentes", "CAMINHO");
            naoProcessado = ini.Read("Nao Processado", "CAMINHO");
            processado = ini.Read("Processado", "CAMINHO");

            pbTestaComunicacoes.Minimum = 0;
            pbTestaComunicacoes.Maximum = 3;
            pbTestaComunicacoes.Value = 0;
            pbTestaComunicacoes.Visibility = Visibility.Visible;

            // 1️⃣ Valida Acesso Pasta Matriz
            if (Directory.Exists(pastaMatriz))
            {
                TbPastaMatrizError.Text = "";
                DotPastaMatriz.Fill = Brushes.Green;
                await Task.Delay(1);
            }
            else if (pastaMatriz == "")
            {
                TbPastaMatrizError.Text = $"Verifique a configuração do arquivo config.ini \n {ini.FilePath}";
                TbPastaMatrizError.Foreground = Brushes.Red;
                DotPastaMatriz.Fill = Brushes.Red;

            }
            else
            {
                TbPastaMatrizError.Text = $"Diretório Inválido!\n{pastaMatriz}";
                TbPastaMatrizError.Foreground = Brushes.Red;
                DotPastaMatriz.Fill = Brushes.Red;

            }
            await Task.Delay(500);
            pbTestaComunicacoes.Value++;

            // 2️⃣ Valida arquivos pendentes
            if (Directory.Exists(arquivosPendentes))
            {
                string[] arquivos = Directory.GetFiles(arquivosPendentes, "*.txt");
                int qtdArquivos = arquivos.Length;

                if (qtdArquivos == 0)
                {
                    TbArquivoPendenteError.Text = "";
                    DotArquivoPendente.Fill = Brushes.Green;

                }
                else if (qtdArquivos <= 5)
                {
                    TbArquivoPendenteError.Text = $"Arquivos pendentes: {qtdArquivos}";
                    DotArquivoPendente.Fill = Brushes.Orange;

                }
                
                else
                {
                    TbArquivoPendenteError.Text = $"Arquivos pendentes: {qtdArquivos}";
                    DotArquivoPendente.Fill = Brushes.Red;

                }
            }
            else if (arquivosPendentes == "")
            {
                TbArquivoPendenteError.Text = $"Verifique a configuração do arquivo config.ini \n {ini.FilePath}";
                TbArquivoPendenteError.Foreground = Brushes.Red;
                DotArquivoPendente.Fill = Brushes.Red;
            }
            else
            {
                TbArquivoPendenteError.Text = "Caminho não encontrado";
                TbArquivoPendenteError.Foreground = Brushes.Red;
                DotArquivoPendente.Fill = Brushes.Red;

            }

            await Task.Delay(500);
            pbTestaComunicacoes.Value++;

            // 3 Valida arquivos não processados
            if (Directory.Exists(naoProcessado))
            {
                string[] arquivos = Directory.GetFiles(naoProcessado, "*.txt");
                int qtdArquivos = arquivos.Length;

                if (qtdArquivos == 0)
                {
                    TbArquivoNaoProcessadoError.Text = "";
                    DotNaoProcessado.Fill = Brushes.Green;
                    await Task.Delay(1);
                }
                else if (qtdArquivos <= 5)
                {
                    TbArquivoNaoProcessadoError.Text = $"Arquivos não processados: {qtdArquivos}";
                    DotNaoProcessado.Fill = Brushes.Orange;

                }
                else
                {
                    TbArquivoNaoProcessadoError.Text = $"Arquivos não processados: {qtdArquivos}";
                    DotNaoProcessado.Fill = Brushes.Red;

                }
            }
            else if (naoProcessado == "")
            {
                TbArquivoNaoProcessadoError.Text = $"Verifique a configuração do arquivo config.ini \n {ini.FilePath}";
                TbArquivoNaoProcessadoError.Foreground = Brushes.Red;
                DotNaoProcessado.Fill = Brushes.Red;
            }
            else
            {
                TbArquivoNaoProcessadoError.Text = "Caminho não encontrado";
                TbArquivoNaoProcessadoError.Foreground = Brushes.Red;
                DotNaoProcessado.Fill = Brushes.Red;
            }

            await Task.Delay(500);
            pbTestaComunicacoes.Value++;

            // Finaliza progress bar
            if (pbTestaComunicacoes.Value == 3)
            {
                await Task.Delay(500);
                pbTestaComunicacoes.Value = 0;
                pbTestaComunicacoes.Visibility = Visibility.Hidden;
            }

            // Reabilita botões
            F_HabilitaTab(true);
            F_HabilitaTodosBotoes(true);
            // Tenta pegar um caso específico que tem arquivo incompatível carregado e a função de habilitar é chamada.
            BtnProcessaManual.IsEnabled = F_ValidarArquivo() && !string.IsNullOrEmpty(caminhoCarregado);
        }

        private void BtnLimparArquivo_Click(object sender, RoutedEventArgs e)
        {
            TbCaminhoArquivo.Text = "";
            BtnProcessaManual.IsEnabled = false;

        }

        private void BtnAbrirConfigINI_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string iniPath = ini.FilePath; 
                Process.Start("explorer.exe", $"/select,\"{iniPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o INI: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}