# 🐉 Ragnar: Smart, Recursive Code Reasoning — From Query to Solution

[![.NET](https://img.shields.io/badge/.NET-10.0-blue?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![C#](https://img.shields.io/badge/C%23-14.0-178600?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)


> **Ragnar** (Repository Augmented Generator & Resolver) is an intelligent, extensible system for analyzing, modernizing, and reasoning about .NET codebases using Retrieval-Augmented Generation (RAG), embeddings, and LLMs.

---

## 🌟 Features

- ✅ **Code Embedding Pipeline**: Parse C# files, extract syntax trees, and generate embeddings for code chunks
- ✅ **Vector Search**: Store and query code snippets using Qdrant vector database
- ✅ **LLM Integration**: Leverage Ollama for embedding generation and generative reasoning
- ✅ **Extensible Plugin System**: Load custom question providers via plugins
- ✅ **XML Comment Analysis**: Identify undocumented or poorly documented code
- ✅ **Modernization Guidance**: Ask questions like *"Which methods lack async patterns?"* or *"Where are performance bottlenecks?"*
- ✅ **Console UI**: Beautiful Spectre.Console output with rich styling and progress
- ✅ **Resilience & Retry**: Built-in HTTP resilience, circuit breakers, and timeouts

---

## 🏗️ Architecture Overview

```
┌─────────────────────┐     ┌──────────────────┐     ┌──────────────────────┐
│  Source Code Files  │────>│  Syntax Parser   │────>│  Code Chunker        │
└─────────────────────┘     └──────────────────┘     └──────────────────────┘
                                                     │  • Class/Method level
                                                     │  • Trivia extraction
                                                     │  • Category inference
                                                     ▼
┌─────────────────────┐     ┌──────────────────┐     ┌──────────────────────┐
│  Embedding Service  │<────│  Ollama Client   │<────│  Generator (Ollama)  │
└─────────────────────┘     └──────────────────┘     └──────────────────────┘
         ▲                                       ▼
         │                              ┌──────────────────────┐
         │                              │  Qdrant Vector DB    │
         │                              └──────────────────────┘
         ▼
┌─────────────────────┐
│  Question Engine    │<─────┐
└─────────────────────┘        │
         ▲                    │
         │              ┌─────┴───────┐
         │              │  Plugins    │
         │              └─────────────┘
         ▼
┌─────────────────────┐
│  Orchestrator       │────> LLM Reasoning + Context
└─────────────────────┘
         ▼
┌─────────────────────┐
│  Console Output     │
└─────────────────────┘
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Ollama](https://ollama.com/) (running locally with embedding model, e.g., `ollama pull nomic-embed-text`)
- [Qdrant](https://qdrant.tech/) (local or cloud instance)

### Configuration

Add a `appsettings.json` (or `appsettings.Development.json`) with:

```json
{
  "ApplicationOptions": {
    "SourceDirectory": "./src",
    "OutputDirectory": "./output"
  },
  "EmbeddingOptions": {
    "Host": "localhost",
    "Port": 6333,
    "Model": "nomic-embed-text"
  },
  "OllamaOptions": {
    "Host": "localhost",
    "Port": 11434,
    "TimeoutMinutes": 20
  },
  "FileLoadOptions": {
    "IncludePatterns": [ "*.cs" ],
    "ExcludePatterns": [ "*Test*.cs", "bin/*", "obj/*" ]
  }
}
```

### Run

```bash
dotnet run --project Ragnar.Cli
```

> 📝 **Tip**: Use `dotnet watch run` for development.

---

## 📁 Project Structure

| Project | Description |
|--------|-------------|
| `Ragnar.Core` | Domain models: `Question`, `CodeDocument`, `Point`, `Filter`, etc. |
| `Ragnar.Embedding` | Embedding generation, point building, vector operations |
| `Ragnar.Parsing` | Roslyn syntax tree parsing, chunking, comment extraction |
| `Ragnar.VectorStore` | Qdrant client integration, point storage/retrieval |
| `Ragnar.Plugins` | Plugin system for custom question providers |
| `Ragnar.Cli` | Entry point with DI, host builder, and console UI |

---

## 🧪 Extensibility

### Add Custom Questions

1. Create a class library targeting `.NET 10.0`
2. Implement `IQuestionProvider`:

```csharp
public class MyCustomProvider : IQuestionProvider
{
    public string Name => "My Questions";
    
    public IReadOnlyList<Question> GetQuestions() =>
        new List<Question>
        {
            Question.IsActive(
                "Are there any methods longer than 50 lines?",
                "long-methods",
                QuestionCategory.Refactor
            )
        };
}
```

3. Place `.dll` in `Questions/Plugins/`
4. Ragnar auto-discovers and loads plugins on startup.

---

## 📊 Example Queries

Ask questions like:

- `"Which classes lack XML documentation comments?"`
- `"Where are synchronous I/O calls in async methods?"`
- `"Find methods with cyclomatic complexity > 10."`
- `"Which types use obsolete APIs?"`

Ragnar uses:
- Vector similarity to find relevant code chunks
- LLM reasoning over retrieved context
- Custom filters (e.g., `Comment_Length == 0`)

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|------------|
| Language | C# 14.0 (record structs, `extension` members, `in` parameters) |
| Framework | .NET 10.0 (generic host, DI, options, validation) |
| Parsing | Roslyn (`Microsoft.CodeAnalysis.CSharp`) |
| Embeddings | Ollama + `nomic-embed-text` |
| Vector DB | Qdrant Client (`Qdrant.Client`) |
| Console UI | Spectre.Console (`Spectre.Console`) |
| Resilience | `Microsoft.Extensions.Resilience` (circuit breakers, retries) |
| Logging | Serilog (configurable sinks) |

---

## 🧩 Key C# 14 Features Used

- ✅ **`extension` members** (e.g., `extension(CodeDocument doc)` in `CodeDocumentExtensions`)
- ✅ **`in` parameters** for performance (e.g., `in ReadOnlySpan<char>`)
- ✅ **`record struct`** for immutable domain models
- ✅ **`static abstract interface members`** (for `IQuestionProvider`)
- ✅ **`primary constructors`** and **`target-typed new()`**

---

## 📜 License

This project is licensed under the [MIT License](LICENSE).

---

## 🙌 Contributing

Contributions are welcome! Please:

1. Fork the repo
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

## 📞 Support

- 🐛 [Report bugs](https://github.com/JonPahl/RAGNAR/issues)
- 💡 [Suggest features](https://github.com/JonPahl/RAGNAR/discussions)
- 💬 Join our [Discussions](https://github.com/JonPahl/RAGNAR/discussions)

---

> 🌟 *“The best way to predict the future is to invent it.”*  
> — Ragnar helps you **invent** better code, faster.
```