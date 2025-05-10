using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.InternalExceptions;

[ExcludeFromCodeCoverage]
public class AlreadyHaveThisStateException(string message) : InternalException(message);