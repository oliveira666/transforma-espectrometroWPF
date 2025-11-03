using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;

namespace TransformaEspectrometroWPF.Forms
{
    public class INI
    {
        private readonly string Path;

        public string FilePath => Path;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        // Construtor: usa a pasta onde o executável está rodando
        public INI(string configINI = "config.ini")
        {

            Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configINI);

            // Cria o arquivo com valores padrão se não existir
            if (!File.Exists(Path))
            {
                Write("Pasta Matriz", "CAMINHO", @"C:/temp/relatorios/Pasta matriz");
                Write("Arquivos Pendentes", "CAMINHO", @"C:/temp/relatorios");
                Write("Processado", "CAMINHO", @"C:/temp/relatorios/Processado");
                Write("Nao Processado", "CAMINHO", @"C:/temp/relatorios/Nao Processado");
            }

                string[] Folders = { PastaMatriz, ArquivosPendentes, Processado, NaoProcessado };
                foreach (string folder in Folders)
                    if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder)) Directory.CreateDirectory(folder);
        }
        public string Read(string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 255, Path);
            return temp.ToString();
        }

        // Propriedades de instância para os caminhos
        public string PastaMatriz => Read("Pasta Matriz", "CAMINHO");
        public string ArquivosPendentes => Read("Arquivos Pendentes", "CAMINHO");
        public string Processado => Read("Processado", "CAMINHO");
        public string NaoProcessado => Read("Nao Processado", "CAMINHO");

        // Escreve valor na chave
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, Path);
        }
    }
}