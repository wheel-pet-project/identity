using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.PublicExceptions;

[ExcludeFromCodeCoverage]
public class ValueIsInvalidException(string message = "Value is invalid") : PublicException(message);