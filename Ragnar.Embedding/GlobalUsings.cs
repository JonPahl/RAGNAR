global using System.Collections.Concurrent;
global using System.Runtime.CompilerServices;
global using System.Runtime.Loader;

global using Ardalis.GuardClauses;

global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.Extensions.AI;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Options;
global using Microsoft.SemanticKernel.Text;

global using OllamaSharp;

global using Qdrant.Client;
global using Qdrant.Client.Grpc;

global using Ragnar.Abstractions;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Enums;
global using Ragnar.Core.Model;
global using Ragnar.Core.Options;
global using Ragnar.Core.Utils;
global using Ragnar.Embedding.Chunker;
global using Ragnar.Embedding.Factory;
global using Ragnar.Embedding.Pipeline.Services;
global using Ragnar.Embedding.UnitOfWork;
global using Ragnar.Plugins;

global using Serilog;

global using Spectre.Console;
