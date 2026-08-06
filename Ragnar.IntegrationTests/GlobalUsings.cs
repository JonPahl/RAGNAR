global using Microsoft.Extensions.Options;

global using Qdrant.Client.Grpc;

global using Ragnar.Core;
global using Ragnar.Core.Model;
global using Ragnar.Core.Options;
global using Ragnar.Embedding.Chunker;
global using Ragnar.Embedding.Factory;
global using Ragnar.Embedding.UnitOfWork;
global using Ragnar.Extensions;
global using Ragnar.Models;
global using Ragnar.Output;
global using Ragnar.Plugins;
global using Ragnar.Questions.Questions;
global using Ragnar.Questions.Questions.Filters;
global using Ragnar.Utils;

global using System.Collections.Immutable;

global using FluentAssertions;

global using Spectre.Console;
global using Ragnar.Core.Validation;
global using Ragnar.Embedding;
global using System.Reflection;
global using Moq;
global using OllamaSharp;
global using OllamaSharp.Models;
global using Ragnar.Core.Interface;
global using Ragnar.Ollama;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Interfaces;
global using Ragnar.RagPipeline;
global using Ragnar.Services;
global using System.IO.Enumeration;
