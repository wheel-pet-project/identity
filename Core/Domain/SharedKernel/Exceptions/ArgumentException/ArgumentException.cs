using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.ArgumentException;

[ExcludeFromCodeCoverage]
public class ArgumentException(string message) : Exception(message);