using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;

[ExcludeFromCodeCoverage]
public class DomainRulesViolationException(string message) : Exception(message);