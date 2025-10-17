using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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

                CriarPastasPadrao();
            }
        }

        // Lê valor da chave
        public string Read(string section, string key)
        {
            StringBuilder temp = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", temp, 255, Path);
            return temp.ToString();
        }

        // Escreve valor na chave
        public void Write(string section, string key, string value)
        {
            WritePrivateProfileString(section, key, value, Path);
        }

        // Cria as pastas configuradas no INI (se não existirem)
        private void CriarPastasPadrao()
        {
            string pastaMatriz = Read("Pasta Matriz", "CAMINHO");
            string arquivosPendentes = Read("Arquivos Pendentes", "CAMINHO");
            string processado = Read("Processado", "CAMINHO");
            string naoProcessado = Read("Nao Processado", "CAMINHO");

            if (!string.IsNullOrWhiteSpace(pastaMatriz)) Directory.CreateDirectory(pastaMatriz);
            if (!string.IsNullOrWhiteSpace(arquivosPendentes)) Directory.CreateDirectory(arquivosPendentes);
            if (!string.IsNullOrWhiteSpace(processado)) Directory.CreateDirectory(processado);
            if (!string.IsNullOrWhiteSpace(naoProcessado)) Directory.CreateDirectory(naoProcessado);
        }
    }
}
