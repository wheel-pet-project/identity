using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.PasswordRecoverTokenAggregate;

public class PasswordRecoverToken
{
    private PasswordRecoverToken(){}

    private PasswordRecoverToken(string recoverTokenHash, Account account) : this()
    {
        RecoverTokenHash = recoverTokenHash;
        AccountId = account.Id;
        ExpiresAt = DateTime.UtcNow.AddMinutes(15);
        IsAlreadyApplied = false;
    }
    
    
    public Guid AccountId { get; private set; }
    
    public string RecoverTokenHash { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    
    public bool IsAlreadyApplied { get; private set; }
    
    public bool IsValid() => ExpiresAt > DateTime.UtcNow && IsAlreadyApplied == false;
    
    public static PasswordRecoverToken Create(string recoverTokenHash, Account account)
    {
        if (string.IsNullOrEmpty(recoverTokenHash) || recoverTokenHash.Length != 60) 
            throw new ValueOutOfRangeException("Recover token hash is invalid or null");
        if (account == null) throw new ValueIsRequiredException("Account is null");
        
        return new PasswordRecoverToken(recoverTokenHash, account);
    }
}
