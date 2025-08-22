using System.Diagnostics.CodeAnalysis;
using FluentResults;

namespace Core.Domain.SharedKernel.Errors;

[ExcludeFromCodeCoverage]
public class TransactionFail(string message, Exception exception) : Error(message)
{
    public Exception Exception { get; } = exception;
};