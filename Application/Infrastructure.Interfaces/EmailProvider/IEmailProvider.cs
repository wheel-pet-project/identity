namespace Application.Infrastructure.Interfaces.EmailProvider;

public interface IEmailProvider
{
    Task SendVerificationEmail(string toAddress, 
        Guid accountId, Guid verificationId);
}