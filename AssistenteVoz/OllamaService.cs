using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Vosk;

namespace AssistenteVoz
{
    public  class OllamaService
    {

        private static readonly HttpClient http = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };


        public async Task<string> PerguntarOllama(string texto)
        {
            var requisicao = new
            {
                model = "qwen3:4b",
                stream = false,
                messages = new[]
                {
                    new { role = "user", content = texto }
                }
            };

            using var resposta = await http.PostAsJsonAsync(
                "http://localhost:11434/api/chat",
                requisicao
            );

            resposta.EnsureSuccessStatusCode();

            string conteudo = await resposta.Content.ReadAsStringAsync();

            using var json = JsonDocument.Parse(conteudo);

            return json.RootElement
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "Não consegui gerar uma resposta.";
        }
    }
}
