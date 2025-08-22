using System.Diagnostics.CodeAnalysis;
using FluentResults;

namespace Core.Domain.SharedKernel.Errors;

[ExcludeFromCodeCoverage]
public class AlreadyExists(string message) : Error(message);