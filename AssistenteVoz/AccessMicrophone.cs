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
using System.Windows.Forms;
using System.Threading;

 public class AccessMicrophone
{

    private static readonly object timeoutLock = new();
    private static CancellationTokenSource? timeoutComandoCts;

    public static VoskRecognizer pathOllama()
    {
        string modelPath =
          "modelo/vosk-model-small-pt-0.3";

        var model = new Model(modelPath);
        var rec = new VoskRecognizer(model, 16000);

        return rec;

    }

    public static void OnMicrophone()
    {
        Console.Beep(800, 150);
        Thread.Sleep(100);
        Console.Beep(1000, 150);
    }


     public static async Task ExecuteDelayAsync(Action function, int temp)
     {
        await Task.Delay(temp);
        function();
     }

     public static bool CancelCommandWaitTime()
     {
         lock (timeoutLock)
         {
             timeoutComandoCts?.Cancel();
             timeoutComandoCts = null;
             return true;
         }
         return false;
     }
}