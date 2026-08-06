global using System.Collections.Immutable;
global using System.Diagnostics;
global using System.Reflection;

global using Microsoft.Extensions.Options;

global using Moq;

global using OllamaSharp;
global using OllamaSharp.Models;

global using Qdrant.Client.Grpc;

global using Ragnar.Branding;
global using Ragnar.Core;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Interface;
global using Ragnar.Core.Model;
global using Ragnar.Core.Options;
global using Ragnar.Core.Utils;
global using Ragnar.Embedding;
global using Ragnar.Embedding.Chunker;
global using Ragnar.Extensions;
global using Ragnar.Factory;
global using Ragnar.Interfaces;
global using Ragnar.Plugins;
global using Ragnar.Questions;
global using Ragnar.Questions.Questions;
global using Ragnar.Utils;

global using RAGNAR.OutputResponse;

global using Spectre.Console;
global using Spectre.Console.Rendering;

global using AssemblyExtensions = Ragnar.Branding.AssemblyExtensions;

global using FluentAssertions;
global using Ragnar.Core.Validation;
global using Ragnar.Questions.Questions.Filters;
