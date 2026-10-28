
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Speech.Synthesis;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using NAudio.Wave;
using Vosk;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using AssistenteVoz;
using System.Windows.Forms;
using System.Threading;

internal class Program
{
    private static readonly HttpClient http = new()
    {
        Timeout = TimeSpan.FromMinutes(5)
    };

    public static SpeechSynthesizer voice = new();
    private static WaveIn? mic;
    private static VoskRecognizer? reconhecedor;
    private static int processando = 0;
    private static readonly OllamaService ollama = new OllamaService();
    private static readonly OpenProgram program = new OpenProgram();
    private static AvatarForms? avatar;
    private static bool awaitingCommand = false;
    private static CancellationTokenSource? timeoutComandoCts;
    private static readonly object timeoutLock = new();


    private static async Task Main()
    {
        string[] names = { 
            "Assistente Disponível!", 
            "Pode me chamar no microfone!",
        };

        OpenAvatar();
        ConfigureVoice();

        Console.WriteLine("Assistente Iniciado!");
        Console.WriteLine("Para sair, pressione ENTER.");

        try
        {
            voice.Rate = 2;
            voice.Volume = 100;


            try
            {
                voice.SelectVoiceByHints(
                    VoiceGender.Male,
                    VoiceAge.Adult,
                    0,
                    new System.Globalization.CultureInfo("pt-BR"));
            }

            catch
            {
                Console.WriteLine(
                    "Aviso: voz pt-BR não encontrada. Usando voz padrão.");
            }
           

            Vosk.Vosk.SetLogLevel(-1);

            reconhecedor = AccessMicrophone.pathOllama();

            mic = new WaveIn
            {
                WaveFormat = new WaveFormat(16000, 16, 1),
                BufferMilliseconds = 250
            };

            mic.DataAvailable += AoReceberAudio;
            AccessMicrophone.ExecuteDelayAsync(avatar.HideAvatar, 10000);

            foreach (string name in names)
            {
                voice.SpeakAsync(name);
            }

            Console.ReadLine();
           
            mic.Dispose();
            voice.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro: {ex.Message}");
        }
    
    }


    private static void AoReceberAudio(object? sender, WaveInEventArgs e)
    {
        if (Volatile.Read(ref processando) == 1)
            return;

        if (reconhecedor == null)
            return;
        
        try
        {
           
            if (!reconhecedor.AcceptWaveform(
                e.Buffer, e.BytesRecorded))
                return;

            using var json = JsonDocument.Parse(
                reconhecedor.Result());

            string text = json.RootElement
                .GetProperty("text").GetString() ?? "";

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (!awaitingCommand)
            {

                if (!text.Contains(
                    "assistente",
                    StringComparison.OrdinalIgnoreCase))
                    return;

                avatar?.ShowAvatar();
                if (Interlocked.CompareExchange(
                    ref processando, 1, 0) != 0)
                    return;

                _ = Task.Run(() =>
                {
                    try
                    {
                        voice.SpeakAsync("Pois não? Pode falar.");
                        awaitingCommand = StartWaitTimeCommand();
 
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Erro ao ativar: {ex.Message}");
                    }
                    finally
                    {
               
                        Interlocked.Exchange(ref processando, 0);
                    }
                });

                return;
            }

            if (Interlocked.CompareExchange(
                ref processando, 1, 0) != 0)
                return;

            _ = Task.Run(async () =>
            {
                try
                {
                    awaitingCommand = AccessMicrophone.CancelCommandWaitTime();
                    awaitingCommand = false;

                    Console.WriteLine($"Você: {text}");

                    if (OpenProgram.ExecutarComando(text, voice))
                    {
                        voice.SpeakAsync("Deseja mais alguma coisa?!");
                        awaitingCommand = StartWaitTimeCommand();
                       
                        return;
                    }

                    if (text.Contains("pode sair",
                          StringComparison.OrdinalIgnoreCase) ||
                          text.Contains("não",
                          StringComparison.OrdinalIgnoreCase) ||
                           text.Contains("saia",
                          StringComparison.OrdinalIgnoreCase))
                    {
                        awaitingCommand = false;
                        voice.SpeakAsync("Ok! Se preciar estarei aqui!");

                        AccessMicrophone.ExecuteDelayAsync(avatar.HideAvatar, 10000);
                        return;
                    }

                    if (text.Contains("encerrar",
                            StringComparison.OrdinalIgnoreCase) ||
                        text.Contains("desligar assistente",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(
                            "Para sair, pressione ENTER.");
                        return;
                    }

                    
                    string resposta = await ollama.PerguntarOllama(text);
                    voice.SpeakAsync(resposta);
                    awaitingCommand = true; 
                }

                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"Erro na resposta: {ex.Message}");
                }
                finally
                {
                    Interlocked.Exchange(ref processando, 0);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro no áudio: {ex.Message}");
        }
    }

    private static void OpenAvatar()
    {
        var thread = new Thread(() =>
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            avatar = new AvatarForms();
            Application.Run(avatar);

        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = false;
        thread.Start();
    }
    
    static void ConfigureVoice()
    {

        voice.SpeakStarted += (sender, e) =>
        {
            avatar?.StartFala();
            mic.StopRecording();
        };

        voice.SpeakCompleted += (sender, e) =>
        {
            avatar?.StopFala();
            AccessMicrophone.ExecuteDelayAsync(mic.StartRecording, 5000);
        };
    }

    private static bool StartWaitTimeCommand()
    {
        //awaitingCommand = true;
        CancellationTokenSource cts;

        lock (timeoutLock)
        {
            timeoutComandoCts?.Cancel();

            cts = new CancellationTokenSource();
            timeoutComandoCts = cts;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(20),
                    cts.Token);

                lock (timeoutLock)
                {
                    if (timeoutComandoCts != cts)
                        return;

                    timeoutComandoCts = null;
                }

                if (awaitingCommand)
                {
                    awaitingCommand = false;
                    AccessMicrophone.ExecuteDelayAsync(avatar.HideAvatar, 10000);
                    voice.SpeakAsync(
                        "Olá! Você ainda está ai? Vou voltar ao modo de espera .");
                }
            }
            finally
            {
                cts.Dispose();
            }
        });
        return true;
    }
}
