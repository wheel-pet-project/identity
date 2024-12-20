using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.PasswordRecoverTokenAggregate;

public class PasswordRecoverToken : Aggregate
{
    private PasswordRecoverToken(){}

    private PasswordRecoverToken(string recoverTokenHash, Account account) : this()
    {
        Id = Guid.NewGuid();
        RecoverTokenHash = recoverTokenHash;
        AccountId = account.Id;
        ExpiresAt = DateTime.UtcNow.AddMinutes(15);
        IsAlreadyApplied = false;
    }
    
    
    public Guid Id { get; private set; }
    
    public Guid AccountId { get; private set; }
    
    public string RecoverTokenHash { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    
    public bool IsAlreadyApplied { get; private set; }
    
    public bool IsValid() => ExpiresAt > DateTime.UtcNow && IsAlreadyApplied == false;
    
    public void Apply() => IsAlreadyApplied = true;
    
    public void AddCreatedDomainEvent(Guid recoverToken, string email) => 
        AddDomainEvent(new PasswordRecoverTokenCreatedDomainEvent(recoverToken, email));

    public static PasswordRecoverToken Create(Account account, string recoverTokenHash)
    {
        if (!ValidatePasswordRecoverToken(recoverTokenHash)) 
            throw new ValueOutOfRangeException("Recover token hash is invalid");
        if (account == null) throw new ValueIsRequiredException("Account is null");
        
        return new PasswordRecoverToken(recoverTokenHash, account);
    }

    private static bool ValidatePasswordRecoverToken(string recoverTokenHash)
    {
        const int hashLength = 60;
        if (recoverTokenHash == null) throw new ValueIsRequiredException("Recover token hash is null");
        return recoverTokenHash.Length == hashLength;
    }
}
