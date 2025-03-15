using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.AlreadyHaveThisState;

[ExcludeFromCodeCoverage]
public class AlreadyHaveThisStateException(string message) : Exception(message);