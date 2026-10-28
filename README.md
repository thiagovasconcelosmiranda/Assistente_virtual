# Assistente Virtual em C#

Assistente virtual para Windows desenvolvido em C#, com reconhecimento de voz, inteligência artificial local e respostas faladas.

## 🚀 Tecnologias utilizadas

- **C# / .NET 10** — desenvolvimento da aplicação.
- **Vosk** — reconhecimento de voz pelo microfone.
- **Ollama** — execução local de modelos de inteligência artificial.
- **System.Speech** — síntese de voz.
- **NAudio** — captura e gerenciamento de áudio.

## 📋 Pré-requisitos

Antes de executar o projeto, verifique se você possui:

- Windows 10 ou 11.
- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- [Ollama](https://ollama.com/download) instalado.
- Um microfone conectado e funcionando.
- O modelo de reconhecimento de voz Vosk em português.

## ⚙️ Instalação e configuração

### 1. Baixe ou clone o projeto

Abra o terminal na pasta do projeto:

```powershell
cd C:\Users\THIAGO\AssistenteVoz
```

### 2. Baixe o modelo de IA

No terminal, execute:

```powershell
ollama pull llama3.2:3b
```

Verifique se o modelo está disponível:

```powershell
ollama list
```

O nome do modelo configurado no código deve corresponder ao nome exibido nessa lista.

### 3. Configure o modelo Vosk

Baixe um modelo de reconhecimento de voz em português no site oficial:

https://alphacephei.com/vosk/models

Extraia o modelo para a pasta esperada pelo código. Por exemplo:

```text
AssistenteVoz/
├── modelo/
│   └── vosk-model-small-pt-0.3/
├── Program.cs
├── OllamaService.cs
├── OpenProgram.cs
└── AssistenteVoz.csproj
```

Confira se o caminho configurado no código corresponde à pasta extraída.

### 4. Inicie o Ollama

Abra o terminal e execute:

```powershell
ollama serve
```

Se o Ollama já estiver em execução, não é necessário iniciá-lo novamente.

### 5. Restaure as dependências

Na pasta do projeto, execute:

```powershell
dotnet restore
```

### 6. Execute o assistente

Inicie o programa com:

```powershell
dotnet run
```

Aguarde a inicialização e verifique se o microfone, o modelo Vosk e o Ollama foram

