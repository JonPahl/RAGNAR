global using System.Buffers;
global using System.Collections.Concurrent;
global using System.Collections.Immutable;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Diagnostics.CodeAnalysis;
global using System.IO.Enumeration;
global using System.Reflection;
global using System.Text;
global using System.Text.RegularExpressions;

global using Ardalis.GuardClauses;

global using FluentValidation;

global using Microsoft.Agents.AI;
global using Microsoft.Extensions.AI;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Http.Resilience;
global using Microsoft.Extensions.Options;

global using OllamaSharp;
global using OllamaSharp.Models;
global using OllamaSharp.Models.Chat;

global using Polly;
global using Polly.Retry;

global using Qdrant.Client;
global using Qdrant.Client.Grpc;

global using Ragnar;
global using Ragnar.Abstractions;
global using Ragnar.Abstractions.Contracts;
global using Ragnar.Abstractions.Embedding;
global using Ragnar.Abstractions.IO;
global using Ragnar.Abstractions.Rendering;
global using Ragnar.Branding;
global using Ragnar.Builder;
global using Ragnar.Core;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Enums;
global using Ragnar.Core.Models;
global using Ragnar.Core.Options;
global using Ragnar.Core.Rendering;
global using Ragnar.Core.Validation;
global using Ragnar.Embedding.Chunker;
global using Ragnar.Embedding.Factory;
global using Ragnar.Embedding.Parsers;
global using Ragnar.Embedding.Pipeline;
global using Ragnar.Embedding.Pipeline.Services;
global using Ragnar.Embedding.Pipeline.Stages;
global using Ragnar.Embedding.VectorStore;
global using Ragnar.Extensions;
global using Ragnar.Factory;
global using Ragnar.Ollama;
global using Ragnar.Output;
global using Ragnar.OutputResponse;
global using Ragnar.Pipeline;
global using Ragnar.Plugins;
global using Ragnar.Prompts;
global using Ragnar.Questions;
global using Ragnar.Questions.Filters;
global using Ragnar.Questions.Interfaces;
global using Ragnar.Questions.Sources;
global using Ragnar.RagPipeline;
global using Ragnar.Services;
global using Ragnar.Stages;
global using Ragnar.Stages.Embedding;
global using Ragnar.Stages.Questions;
global using Ragnar.Utils;

global using Spectre.Console;

global using ChatRole = OllamaSharp.Models.Chat.ChatRole;
global using IQuestionSource = Ragnar.Core.IQuestionSource;
global using Serilog;
