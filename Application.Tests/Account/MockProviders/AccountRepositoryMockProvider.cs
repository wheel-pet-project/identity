using Application.Infrastructure.Interfaces.Repositories;
using Domain.AccountAggregate;
using Moq;

namespace Application.Tests.Account.MockProviders;

public class AccountRepositoryMockProvider
{
    private Mock<IAccountRepository> _accountRepositoryMock;
    private readonly AccountFactory _accountFactory;

    public IAccountRepository BuildRepository( 
        bool createAccountSuccess = true,
        bool deleteConfirmationRecordSuccess = true,
        bool createRefreshTokenSuccess = true,
        bool updateStatusSuccess = true,
        bool updatePasswordSuccess = true,
        bool updateEmailSuccess = true)
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        throw new NotImplementedException();
    }
}