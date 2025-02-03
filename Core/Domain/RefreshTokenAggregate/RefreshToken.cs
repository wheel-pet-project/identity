using Core.Domain.AccountAggregate;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.RefreshTokenAggregate;

public class RefreshToken
{
    private static readonly TimeSpan DefaultRefreshTokenLifetime = TimeSpan.FromDays(21);
    
    private RefreshToken(){}

    private RefreshToken(Account account, DateTime issueDateTime, DateTime expiresAt) : this()
    {
        Id = Guid.NewGuid();
        AccountId = account.Id;
        IssueDateTime = issueDateTime;
        ExpiresAt = expiresAt; 
        IsRevoked = false;
    }
    
    
    public Guid Id { get; private set; }
    
    public Guid AccountId { get; private set; }
    
    public DateTime IssueDateTime { get; private set; }
    
    public DateTime ExpiresAt { get; private set; }
    
    public bool IsRevoked { get; private set; }

    public bool IsValid(TimeProvider timeProvider)
    {
        if (timeProvider == null) throw new ValueIsRequiredException($"{nameof(timeProvider)} cannot be null");
        
        return !IsRevoked && ExpiresAt > timeProvider.GetUtcNow().UtcDateTime;
    }

    public void Revoke() => IsRevoked = true;

    public static RefreshToken Create(Account account, TimeProvider timeProvider)
    {
        if (timeProvider == null) throw new ValueIsRequiredException($"{nameof(timeProvider)} cannot be null");
        if (account == null) throw new ValueIsRequiredException($"{nameof(account)} cannot be null");

        var issueDateTime = timeProvider.GetUtcNow().UtcDateTime;
        var expiresAt = timeProvider.GetUtcNow().Add(DefaultRefreshTokenLifetime).UtcDateTime;
        
        return new RefreshToken(account, issueDateTime, expiresAt);
    }
}