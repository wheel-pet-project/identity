using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.PublicExceptions;

[ExcludeFromCodeCoverage]
public class ValueOutOfRangeException(string message = "Value out of range") : PublicException(message);