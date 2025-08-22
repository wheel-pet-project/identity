using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.PublicExceptions;

[ExcludeFromCodeCoverage]
public class PublicException(string message) : Exception(message);