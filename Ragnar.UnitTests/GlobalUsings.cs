global using System.Collections.Immutable;
global using System.Diagnostics;

global using Microsoft.Extensions.Options;

global using Moq;

global using OllamaSharp.Models;

global using Qdrant.Client;

global using Ragnar.Abstractions;
global using Ragnar.Branding;
global using Ragnar.Builder;
global using Ragnar.Contracts;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Model;
global using Ragnar.Core.Options;
global using Ragnar.Core.Utils;
global using Ragnar.Embedding;
global using Ragnar.Embedding.Pipeline;
global using Ragnar.Embedding.UnitOfWork;
global using Ragnar.Extensions;
global using Ragnar.Output;
global using Ragnar.OutputResponse;
global using Ragnar.Plugins;
global using Ragnar.Questions;
global using Ragnar.Services;
global using Ragnar.Stages;
global using Ragnar.Utils;
global using Ragnar.Validator;

global using Spectre.Console;
global using Spectre.Console.Rendering;
