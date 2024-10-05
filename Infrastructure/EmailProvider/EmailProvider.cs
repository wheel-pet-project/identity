using Application.Infrastructure.Interfaces.EmailProvider;
using FluentEmail.Core;
using FluentEmail.Core.Defaults;

namespace Infrastructure.EmailProvider;

public class EmailProvider(IFluentEmail fluentEmail) : IEmailProvider
{
    public async Task SendVerificationEmail(string toAddress, 
        Guid accountId, Guid verificationId)
    {
        var response = await fluentEmail
            .To(toAddress)
            .Subject("Подтверждение электронной почты")
            .Body($"""
                  <h3>Подтверждение электронной почты</h3>
                  <p>Для подтверждения электронной почты перейдите по ссылке:</p>
                  <a href="http://localhost:6000/api/account/verify?accountId={accountId}&verificationId={verificationId}">Ссылка для подтверждения</a>
                  """, true)
            .SendAsync();
    }
}