
using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Speech.Synthesis;

public class OpenProgram
{

    public static bool ExecutarComando(string text, SpeechSynthesizer voice)
    {
            string t = text.ToLowerInvariant().Trim();

            voice.SpeakAsync(text);

            if (Contem(t, "bloco de notas", "bloco de nota", "notepad"))
                return AbrirPrograma("notepad.exe", "Bloco de Notas", voice);

            if (Contem(t, "calculadora"))
                return AbrirPrograma("calc.exe", "Calculadora", voice);

            if (Contem(t, "navegador google", "google chrome"))
                return AbrirPrograma("chrome.exe", "Google Chrome", voice);

            if (Contem(t, "visual estúdio code", "code"))
                return AbrirPrograma("code.exe", "Visual Studio Code", voice);

            if (Contem(t, "visual stúdio"))
                return AbrirPrograma("devenv.exe", "Visual Studio", voice);

            if (Contem(t, "controle financeiro"))
            return AbrirPrograma("financeiro.exe", "Controle Financeiro", voice);

        if (Contem(t, "acessar câmeras", "câmeras"))
            return AbrirPrograma("Security Monitor Pro.exe", "Segurança", voice);
        if (Contem(t, "youtube"))
                return AbrirSite("https://www.youtube.com", "YouTube", voice);

            if (Contem(t, "google") &&
                !Contem(t, "pesquise", "pesquisar", "pesquisa"))
                return AbrirSite("https://www.google.com", "Google", voice);

            if (Contem(t, "pesquise", "pesquisar", "pesquisa",
                          "procure na internet", "busque na internet"))
            {
                string consult = ExtrairConsulta(t);

                if (string.IsNullOrWhiteSpace(consult))
                    consult = t;

                string url = "https://www.google.com/search?q=" +
                             Uri.EscapeDataString(consult);

                return AbrirSite(url,"Pesquisa na internet", voice);
            }

            if (Contem(t, "downloads", "pasta de downloads"))
                return AbrirPasta(
                    Path.Combine(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.UserProfile),
                        "Downloads"), voice);

            if (Contem(t, "área de trabalho", "area de trabalho",
                          "desktop") && Contem(t, "abra", "abrir", "acessar"))
                return AbrirPasta(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.DesktopDirectory), voice);

            // 5. Criar arquivo de texto
            if (Contem(t, "crie um arquivo", "criar um arquivo",
                          "crie um texto", "criar um texto"))
            {
                string desktop = Environment.GetFolderPath(
                    Environment.SpecialFolder.DesktopDirectory);

                string path = Path.Combine(desktop, "Anotacoes.txt");

                File.WriteAllText(path, "");

                return AbrirArquivo(path, voice);
            }

            // 6. Informar hora
            if (Contem(t, "que horas são", "que horas sao",
                          "horas agora", "hora atual"))
            {

                voice.Speak($"Agora são {DateTime.Now:HH:mm}.");
                Console.WriteLine($"Agora são {DateTime.Now:HH:mm}.");
            
                return true;
            }
            return false;
        }

        private static bool Contem(string text, params string[] term)
        {
            return term.Any(term => text.Contains(term));
        }

        private static string ExtrairConsulta(string text)
        {
            string result = Regex.Replace(
                text,
                @"\b(por favor|pesquise|pesquisar|pesquisa|procure na internet|busque na internet|na internet|no google|para mim)\b",
                "",
                RegexOptions.IgnoreCase);

            return result.Trim(' ', '.', ',', '?', '!');
        }

        private static bool AbrirSite(string url, string name, SpeechSynthesizer voice)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });

                voice.SpeakAsync($"Aguade! Estou Abrindo {name}.");
                return true;
            }
            catch (Exception ex)
            {
             voice.SpeakAsync($"Erro ao abrir {name}: {ex.Message}");
                return true;
            }
        }

        private static bool AbrirPasta(string path, SpeechSynthesizer voice)
        {
            if (!Directory.Exists(path))
            {
                voice.SpeakAsync($" Pasta não encontrada: {path}");
                return true;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });

            return true;
        }

        private static bool AbrirArquivo(string path, SpeechSynthesizer voice)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });

            voice.SpeakAsync($"Aguarde! Arquivo criado: {path}");
            return true;
        }

        private static bool AbrirPrograma(string program, string name, SpeechSynthesizer voice)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = program,
                    UseShellExecute = true
                });


             voice.SpeakAsync($"Aguarde! Estou Abrindo {name}.");
                return true;
            }
            catch
            {
            voice.SpeakAsync(
                    $"Não consegui abrir {name}. Verifique se está instalado e disponível no PATH.");

                return true;
            }
      
         }


    }



   