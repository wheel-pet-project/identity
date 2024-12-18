using Core.Domain.ConfirmationTokenAggregate.DomainEvents;
using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.ConfirmationTokenAggregate;

public class ConfirmationToken : Aggregate
{
    private ConfirmationToken(){}

    private ConfirmationToken(Guid accountId, string confirmationTokenHash) : this()
    {
        AccountId = accountId;
        ConfirmationTokenHash = confirmationTokenHash;
    }
    
    public Guid AccountId { get; private set; }
    
    public string ConfirmationTokenHash { get; private set; }

    // todo: add tests
    public void AddCreatedDomainEvent(Guid confirmationToken) => 
        AddDomainEvent(new ConfirmationTokenCreatedDomainEvent(confirmationToken));

    public static ConfirmationToken Create(Guid accountId, string confirmationTokenHash)
    {
        if (accountId == Guid.Empty) throw new ValueIsRequiredException("Account id is empty");
        if (!ValidateConfirmationTokenHash(confirmationTokenHash))
            throw new ValueOutOfRangeException("Confirmation token hash is invalid, hash length must be 60");
        
        return new ConfirmationToken(accountId, confirmationTokenHash);
    }

    private static bool ValidateConfirmationTokenHash(string tokenHash)
    {
        const int hashLength = 60;
        if (tokenHash == null) throw new ValueIsRequiredException("Confirmation token hash is null");
        return tokenHash.Length == hashLength;
    }
}