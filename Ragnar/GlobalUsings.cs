global using System.Buffers;
global using System.Collections.Immutable;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.IO.Enumeration;
global using System.Reflection;
global using System.Text;

global using Ardalis.GuardClauses;

global using FileQuestionProvider;

global using FluentValidation;

global using Microsoft.Agents.AI;
global using Microsoft.Extensions.AI;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Http.Resilience;
global using Microsoft.Extensions.Options;
global using Microsoft.SemanticKernel.Embeddings;

global using OllamaSharp;
global using OllamaSharp.Models;

global using Polly;

global using Qdrant.Client;
global using Qdrant.Client.Grpc;

global using Ragnar;
global using Ragnar.Abstractions;
global using Ragnar.Branding;
global using Ragnar.Builder;
global using Ragnar.Contracts;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Enums;
global using Ragnar.Core.Model;
global using Ragnar.Core.Options;
global using Ragnar.Core.Utils;
global using Ragnar.Embedding.Embedding;
global using Ragnar.Embedding.Factory;
global using Ragnar.Embedding.Pipeline;
global using Ragnar.Embedding.Pipeline.Interface;
global using Ragnar.Embedding.Pipeline.Stages;
global using Ragnar.Embedding.UnitOfWork;
global using Ragnar.Extensions;
global using Ragnar.Factory;
global using Ragnar.Interfaces;
global using Ragnar.Models;
global using Ragnar.Ollama;
global using Ragnar.Output;
global using Ragnar.OutputResponse;
global using Ragnar.Plugins;
global using Ragnar.Questions;
global using Ragnar.Questions.Questions;
global using Ragnar.RagPipeline;
global using Ragnar.Services;
global using Ragnar.Stages;
global using Ragnar.Stages.Questions;
global using Ragnar.Utils;

global using Serilog;

global using Spectre.Console;
global using OllamaSharp.Models.Chat;
global using System.Collections.Concurrent;
global using Ragnar.Embedding.Pipeline.Services;
