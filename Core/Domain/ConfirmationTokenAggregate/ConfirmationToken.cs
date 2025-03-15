using Core.Domain.SharedKernel.Exceptions.ArgumentException;

namespace Core.Domain.ConfirmationTokenAggregate;

public class ConfirmationToken
{
    private ConfirmationToken()
    {
    }

    private ConfirmationToken(Guid accountId, string confirmationTokenHash) : this()
    {
        AccountId = accountId;
        ConfirmationTokenHash = confirmationTokenHash;
    }


    public Guid AccountId { get; private set; }
    public string ConfirmationTokenHash { get; private set; } = null!;


    public static ConfirmationToken Create(Guid accountId, string confirmationTokenHash)
    {
        if (accountId == Guid.Empty) throw new ValueIsRequiredException($"{nameof(accountId)} cannot empty");
        if (!ValidateConfirmationTokenHash(confirmationTokenHash))
            throw new ValueOutOfRangeException($"{nameof(confirmationTokenHash)} is invalid, hash length must be 60");

        return new ConfirmationToken(accountId, confirmationTokenHash);
    }

    private static bool ValidateConfirmationTokenHash(string tokenHash)
    {
        const int hashLength = 60;
        if (tokenHash == null) throw new ValueIsRequiredException($"{nameof(tokenHash)} cannot be null");
        return tokenHash.Length == hashLength;
    }
}