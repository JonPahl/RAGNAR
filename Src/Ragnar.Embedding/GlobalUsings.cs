global using System.Runtime.CompilerServices;
global using System.Runtime.Loader;

global using Ardalis.GuardClauses;

global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.Extensions.AI;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Microsoft.SemanticKernel.Text;

global using OllamaSharp;

global using Polly;
global using Polly.Retry;

global using Qdrant.Client;
global using Qdrant.Client.Grpc;

global using Ragnar.Abstractions;
global using Ragnar.Abstractions.Embedding;
global using Ragnar.Abstractions.IO;
global using Ragnar.Core;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Enums;
global using Ragnar.Core.Models;
global using Ragnar.Core.Options;
global using Ragnar.Core.Utils;
global using Ragnar.Embedding.Chunker;
global using Ragnar.Embedding.Parsers;

global using Spectre.Console;
