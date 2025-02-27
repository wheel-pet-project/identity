using Core.Application.UseCases.CreateAccount;
using Core.Domain.AccountAggregate;
using Core.Domain.Services;
using Core.Domain.Services.CreateAccountService;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.UseCases;

public class CreateAccountUseCaseShould
{
    private readonly CreateAccountRequest _request = new(Role.Customer, "test@test.com", "+79008007060",
        "somepassword");

    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60), Guid.NewGuid());

    [Fact]
    public async Task ReturnAccountIdForCorrectRequest()
    {
        // Arrange
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureCreateAccountService(_account);
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        useCaseBuilder.ConfigureHasher(new string('*', 60));
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ReturnCorrectErrorIfCreateAccountReturnsFail()
    {
        // Arrange
        var expectedError = new Error("expected error");

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureCreateAccountService(expectedError);
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        useCaseBuilder.ConfigureHasher(new string('*', 60));
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(expectedError, result.Errors.First());
    }

    [Fact]
    public async Task ReturnCorrectErrorResultIfTransactionFails()
    {
        // Arrange
        var expectedError = new Error("expected error");

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureCreateAccountService(_account);
        useCaseBuilder.ConfigureUnitOfWork(Result.Fail(expectedError));
        useCaseBuilder.ConfigureHasher(new string('*', 60));
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(expectedError, result.Errors.First());
    }

    private class UseCaseBuilder
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IOutbox> _outboxMock = new();
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IConfirmationTokenRepository> _confirmationTokenRepositoryMock = new();
        private readonly Mock<ICreateAccountService> _createAccountServiceMock = new();
        private readonly Mock<IHasher> _hasherMock = new();

        public CreateAccountHandler Build()
        {
            return new CreateAccountHandler(_confirmationTokenRepositoryMock.Object,
                _createAccountServiceMock.Object, _accountRepositoryMock.Object, _unitOfWorkMock.Object,
                _outboxMock.Object, _hasherMock.Object);
        }

        // public void ConfigureAccountRepository() =>
        //     _accountRepositoryMock.Setup(x => x.Add(It.IsAny<Account>()));

        public void ConfigureCreateAccountService(Result<Account> createAccountShouldReturn)
        {
            _createAccountServiceMock
                .Setup(x => x.CreateUser(It.IsAny<Role>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<Guid>()))
                .ReturnsAsync(createAccountShouldReturn);
        }

        public void ConfigureUnitOfWork(Result commitShouldReturn)
        {
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);
        }

        public void ConfigureHasher(string generateHashShouldReturn)
        {
            _hasherMock.Setup(x => x.GenerateHash(It.IsAny<string>())).Returns(generateHashShouldReturn);
        }
    }
}