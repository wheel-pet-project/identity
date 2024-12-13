using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.ArgumentException;

[ExcludeFromCodeCoverage]
public class ValueOutOfRangeException(string message = "Value out of range") : ArgumentException(message);