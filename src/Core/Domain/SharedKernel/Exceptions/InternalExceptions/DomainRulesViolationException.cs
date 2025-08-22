using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.InternalExceptions;

[ExcludeFromCodeCoverage]
public class DomainRulesViolationException(string message) : InternalException(message);