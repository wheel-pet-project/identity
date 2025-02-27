using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Domain.SharedKernel;
using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.PasswordRecoverTokenAggregate;

public class PasswordRecoverToken : Aggregate
{
    private static readonly TimeSpan RecoverExpiryTimeSpan = TimeSpan.FromMinutes(15);

    private PasswordRecoverToken()
    {
    }

    private PasswordRecoverToken(string recoverTokenHash, DateTime expiresAt, Account account) : this()
    {
        Id = Guid.NewGuid();
        RecoverTokenHash = recoverTokenHash;
        AccountId = account.Id;
        ExpiresAt = expiresAt;
        IsAlreadyApplied = false;
    }


    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public string RecoverTokenHash { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public bool IsAlreadyApplied { get; private set; }

    public bool IsValid(TimeProvider timeProvider)
    {
        if (timeProvider is null) throw new ValueIsRequiredException($"{nameof(timeProvider)} cannot be null");

        return ExpiresAt > timeProvider.GetUtcNow().UtcDateTime && IsAlreadyApplied == false;
    }

    public void Apply()
    {
        IsAlreadyApplied = true;
    }

    public static PasswordRecoverToken Create(
        Account account,
        Guid recoverTokenGuid,
        string recoverTokenHash,
        TimeProvider timeProvider)
    {
        if (timeProvider == null)
            throw new ValueIsRequiredException($"{nameof(timeProvider)} cannot be null");
        if (recoverTokenGuid == Guid.Empty)
            throw new ValueIsRequiredException($"{nameof(recoverTokenGuid)} cannot be empty");
        if (account == null)
            throw new ValueIsRequiredException($"{nameof(account)} cannot be null");
        if (!ValidatePasswordRecoverToken(recoverTokenHash))
            throw new ValueOutOfRangeException($"{recoverTokenHash} cannot be invalid");

        var expiresAt = timeProvider.GetUtcNow().UtcDateTime.Add(RecoverExpiryTimeSpan);

        var newRecoverToken = new PasswordRecoverToken(recoverTokenHash, expiresAt, account);
        newRecoverToken.AddDomainEvent(new PasswordRecoverTokenCreatedDomainEvent(account.Id, recoverTokenGuid));

        return newRecoverToken;
    }

    private static bool ValidatePasswordRecoverToken(string recoverTokenHash)
    {
        const int hashLength = 60;
        if (recoverTokenHash == null) throw new ValueIsRequiredException($"{recoverTokenHash} cannot be null");
        return recoverTokenHash.Length == hashLength;
    }
}