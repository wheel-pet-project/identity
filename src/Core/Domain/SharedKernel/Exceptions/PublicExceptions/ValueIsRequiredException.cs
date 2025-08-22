using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.PublicExceptions;

[ExcludeFromCodeCoverage]
public class ValueIsRequiredException(string message = "Value is required") : PublicException(message);