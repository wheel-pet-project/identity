using System.Diagnostics.CodeAnalysis;

namespace Core.Domain.SharedKernel.Exceptions.DomainRulesViolationException;

[ExcludeFromCodeCoverage]
public class DomainRulesViolationException(string message, bool isAlreadyInThisState = false) : Exception(message)
{
    public bool IsAlreadyInThisState { get; } = isAlreadyInThisState;
}