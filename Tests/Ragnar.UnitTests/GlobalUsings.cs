global using System.ComponentModel.DataAnnotations;
global using System.Diagnostics;
global using System.Reflection;

global using Ardalis.GuardClauses;

global using FluentValidation.TestHelper;

global using Meziantou.Extensions.Logging.Xunit.v3;

global using Microsoft.Extensions.AI;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Moq;

global using Polly;
global using Polly.Retry;

global using Qdrant.Client;
global using Qdrant.Client.Grpc;

global using Ragnar.Abstractions;
global using Ragnar.Abstractions.Contracts;
global using Ragnar.Abstractions.Embedding;
global using Ragnar.Abstractions.IO;
global using Ragnar.Branding;
global using Ragnar.Builder;
global using Ragnar.Core;
global using Ragnar.Core.ConsoleWriter;
global using Ragnar.Core.Validation;
global using Ragnar.Embedding.Factory;
global using Ragnar.Embedding.Parsers;
global using Ragnar.Embedding.Pipeline;
global using Ragnar.Embedding.Pipeline.Services;
global using Ragnar.Embedding.VectorStore;
global using Ragnar.Exceptions;
global using Ragnar.Extensions;
global using Ragnar.Factory;
global using Ragnar.Middleware;
global using Ragnar.Output;
global using Ragnar.Pipeline;
global using Ragnar.Plugins;
global using Ragnar.Prompts;
global using Ragnar.Questions;
global using Ragnar.Questions.Filters;
global using Ragnar.Questions.Sources;
global using Ragnar.Stages;
global using Ragnar.Stages.Embedding;
global using Ragnar.Stages.Questions;
global using Ragnar.Store;
global using Ragnar.Utils;
global using Ragnar.Validator;

global using Spectre.Console;
global using Spectre.Console.Rendering;

global using AssemblyExtensions = Ragnar.Branding.AssemblyExtensions;
global using ILogger = Microsoft.Extensions.Logging.ILogger;
global using StringExtensions = Ragnar.Embedding.StringExtensions;
global using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
global using Ragnar.Domain.Options;
global using Ragnar.Domain.Models;
global using Ragnar.Domain.Enums;
global using Ragnar.Domain;
