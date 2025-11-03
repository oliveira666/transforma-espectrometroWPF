using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TransformaEspectrometroWPF.Forms
{ 
    public partial class MainWindow : Window
    {
        private INI ini = null!;
        private string caminhoCarregado = string.Empty;
        private string pastaMatriz = "";
        private string arquivosPendentes = "";
        private string processado = "";
        private string naoProcessado = "";
        public MainWindow()
        {
            InitializeComponent();

        }
        public async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ini = new INI();

             pastaMatriz = ini.PastaMatriz;
             arquivosPendentes = ini.ArquivosPendentes;
             processado = ini.Processado;
             naoProcessado = ini.NaoProcessado;

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

        public (int ok, int nok) ContarItens(string path)
        {
            try
            {
                string[] arquivos = Directory.GetFiles(path, "*.txt");
                int ok = 0;
                int nok = 0;

                foreach (var arquivo in arquivos)
                {
                    string[] linhas = File.ReadAllLines(arquivo);
                    if (linhas.Length == 0)
                    {
                        nok++;
                        continue;
                    }

                    string[] cabecalho = linhas[0].Split(';');

                    if (linhas.Length > 2 || !cabecalho[0].Contains("Timestamp"))
                        nok++;
                    else
                        ok++;
                }

                return (ok, nok);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao ler os arquivos: " + ex.Message);
                return (0, 0);
            }
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

            
            TbPastaMatrizError.Visibility = Visibility.Visible;
            TbArquivoNaoProcessadoError.Visibility = Visibility.Visible;
            TbArquivoPendenteError.Visibility = Visibility.Visible;

            
            ini = new INI();

            pbTestaComunicacoes.Minimum = 0;
            pbTestaComunicacoes.Maximum = 4;
            pbTestaComunicacoes.Value = 0;
            pbTestaComunicacoes.Visibility = Visibility.Visible;

            //Default \\nhy.hydro.com\dfs\BR-SAO-ITU\Common\...
            if (!Directory.Exists(pastaMatriz))
            {
                TbPastaMatrizError.Text = $"Diretório não encontrado! Verifique o config.ini\n{pastaMatriz}";
                TbPastaMatrizError.Foreground = Brushes.Red;
                DotPastaMatriz.Fill = Brushes.Red;
            }
            else
            {
                TbPastaMatrizError.Text = "";
                DotPastaMatriz.Fill = Brushes.Green;

            }
            await Task.Delay(500);
            pbTestaComunicacoes.Value++;

            if (!Directory.Exists(processado))
            {
                TbPastaMatrizError.Text = $"Diretório não encontrado! Verifique o config.ini\n{processado}";
                TbPastaMatrizError.Foreground = Brushes.Red;
                DotPastaMatriz.Fill = Brushes.Red;
            }
            else
            {
                TbPastaMatrizError.Text = "";
                DotPastaMatriz.Fill = Brushes.Green;

            }
            await Task.Delay(500);
            pbTestaComunicacoes.Value++;

            // Default c:/temp/relatorios
            if (!Directory.Exists(arquivosPendentes))
            {
                TbArquivoPendenteError.Text = $"Diretório não existe!\n{arquivosPendentes}";
                TbArquivoPendenteError.Foreground = Brushes.Red;
                DotArquivoPendente.Fill = Brushes.Red;
            }
            else
            {

                var itens = ContarItens(arquivosPendentes); 

                if (itens.ok == 0)
                {
                    TbArquivoPendenteError.Text = "";
                    DotArquivoPendente.Fill = Brushes.Green;

                }
                else if (itens.ok < 5)
                {
                    TbArquivoPendenteError.Text = $"Arquivos pendentes: {itens.ok}";
                    DotArquivoPendente.Fill = Brushes.Orange;

                }
                else
                {
                    TbArquivoPendenteError.Text = $"Arquivos pendentes: {itens.ok}";
                    DotArquivoPendente.Fill = Brushes.Red;

                }
                if (itens.nok > 0)
                    TbArquivoPendenteError.Text += $"\nArquivos com erro: {itens.nok}";
            }

            await Task.Delay(500);
            pbTestaComunicacoes.Value++;


            if (!Directory.Exists(naoProcessado))
            {
                var msg = MessageBox.Show($"ERRO: A pasta {naoProcessado} não existe.\n\n Deseja criar uma nova pasta?",
                                          "Mensagem", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (msg == MessageBoxResult.Yes)
                    if (!string.IsNullOrWhiteSpace(naoProcessado)) Directory.CreateDirectory(naoProcessado);

                TbArquivoNaoProcessadoError.Text = $"Diretório não existe!\n{naoProcessado}";
                TbArquivoNaoProcessadoError.Foreground = Brushes.Red;
                DotNaoProcessado.Fill = Brushes.Red;
            }
            else
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


            await Task.Delay(500);
            pbTestaComunicacoes.Value++;

            // Finaliza progress bar
            if (pbTestaComunicacoes.Value >= pbTestaComunicacoes.Maximum)
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