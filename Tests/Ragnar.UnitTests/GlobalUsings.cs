global using System.Reflection;

global using FluentValidation.TestHelper;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;

global using Moq;

global using Qdrant.Client;
global using Qdrant.Client.Grpc;

global using Ragnar.Abstractions;
global using Ragnar.Branding;
global using Ragnar.Core;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Enums;
global using Ragnar.Core.Models;
global using Ragnar.Core.Options;
global using Ragnar.Core.Utils;
global using Ragnar.Core.Validation;
global using Ragnar.Embedding.Parsers;
global using Ragnar.Embedding.Pipeline;
global using Ragnar.Extensions;
global using Ragnar.Factory;
global using Ragnar.Output;
global using Ragnar.Prompts;
global using Ragnar.Questions;
global using Ragnar.Questions.Filters;
global using Ragnar.Questions.Sources;
global using Ragnar.Utils;
global using Ragnar.Validator;

global using Spectre.Console;
global using Spectre.Console.Rendering;
global using System.Diagnostics;
global using Ragnar.Abstractions.IO;
global using Ragnar.Abstractions.Embedding;
global using Ragnar.Stages.Questions;
global using Ragnar.Builder;
global using Ragnar.Pipeline;
global using Polly.Retry;
global using Ragnar.Plugins;
global using Ragnar.Abstractions.Contracts;
global using Ragnar.Stages;
global using Ragnar.Core.Rendering;
global using Ragnar.Core.Rendering.Decorators;
global using AssemblyExtensions = Ragnar.Branding.AssemblyExtensions;
global using Polly;
global using Ragnar.Embedding.Chunker;
global using Ragnar.Embedding.Factory;
global using Microsoft.Extensions.AI;
global using Ragnar.Embedding.Embedding;
global using Serilog;
