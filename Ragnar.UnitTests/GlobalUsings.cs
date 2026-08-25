global using System.Collections.Immutable;
global using System.Diagnostics;
global using System.Reflection;

global using FluentAssertions;

global using Microsoft.Extensions.Options;

global using Moq;

global using Ragnar.Branding;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Model;
global using Ragnar.Core.Options;
global using Ragnar.Core.Utils;
global using Ragnar.Embedding;
global using Ragnar.Embedding.Chunker;
global using Ragnar.Embedding.Factory;
global using Ragnar.Extensions;
global using Ragnar.Factory;
global using Ragnar.Plugins;
global using Ragnar.Questions;
global using Ragnar.Utils;
global using Ragnar.Validator;

global using Spectre.Console;
global using Spectre.Console.Rendering;

global using AssemblyExtensions = Ragnar.Branding.AssemblyExtensions;
global using OllamaSharp.Models;
global using Ragnar.Core.Enums;
global using Ragnar.Core.Interface;
global using Ragnar.Models;
global using Ragnar.Output;
global using Ragnar.Interfaces;
global using Ragnar.RagPipeline;
global using Ragnar.OutputResponse;
global using Ragnar.Questions.Filters;
global using Ragnar.Services;
global using Ragnar.Embedding.Pipeline.Services;
global using Qdrant.Client.Grpc;
global using Qdrant.Client;
global using System.ComponentModel;
global using System.IO.Abstractions.TestingHelpers;
global using System.Reflection.Emit;
global using Ragnar.Embedding._Pipeline;
global using FluentValidation.TestHelper;
