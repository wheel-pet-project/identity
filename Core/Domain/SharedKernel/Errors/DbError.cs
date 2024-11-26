using System.Diagnostics.CodeAnalysis;
using FluentResults;

namespace Core.Domain.SharedKernel.Errors;

[ExcludeFromCodeCoverage]
public class DbError(string message) : Error(message);