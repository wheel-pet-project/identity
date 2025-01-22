using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.RefreshTokenAggregate;

public class RefreshToken
{
    private static readonly TimeSpan DefaultRefreshTokenLifetime = TimeSpan.FromDays(21);
    
    private RefreshToken(){}

    private RefreshToken(Account account) : this()
    {
        Id = Guid.NewGuid();
        AccountId = account.Id;
        IssueDateTime = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow + DefaultRefreshTokenLifetime; 
        IsRevoked = false;
    }
    
    
    public Guid Id { get; private set; }
    
    public Guid AccountId { get; private set; }
    
    public DateTime IssueDateTime { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    
    public bool IsRevoked { get; private set; }

    public bool IsValid() => !IsRevoked && ExpiresAt > DateTime.UtcNow;
    
    public void Revoke() => IsRevoked = true;

    public static RefreshToken Create(Account account)
    {
        if (account == null) throw new ValueIsRequiredException($"{nameof(account)} cannot be null");

        return new RefreshToken(account);
    }
}