using Core.Application.UseCases.ConfirmEmail;
using Core.Domain.AccountAggregate;
using Core.Domain.ConfirmationTokenAggregate;
using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.UseCases;

public class ConfirmEmailUseCaseShould
{
    private readonly ConfirmAccountEmailCommand _command = new(
        Guid.NewGuid(),
        Guid.NewGuid());

    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60), Guid.NewGuid());

    [Fact]
    public async Task ReturnSuccessResultForCorrectRequest()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(_account);
        useCaseBuilder.ConfigureConfirmationTokenRepository(confirmationToken);
        useCaseBuilder.ConfigureHasher(true);
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_command, default);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ReturnFailedResultIfGetConfirmationTokenReturnsNull()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(_account);
        useCaseBuilder.ConfigureConfirmationTokenRepository(null);
        useCaseBuilder.ConfigureHasher(true);
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_command, default);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ReturnFailedResultIfVerifyHashReturnsFalse()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(_account);
        useCaseBuilder.ConfigureConfirmationTokenRepository(confirmationToken);
        useCaseBuilder.ConfigureHasher(false);
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_command, default);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ThrowsDataConsistencyViolationExceptionIfGetByIdReturnsNull()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(null);
        useCaseBuilder.ConfigureConfirmationTokenRepository(confirmationToken);
        useCaseBuilder.ConfigureHasher(true);
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        async Task Act()
        {
            await useCase.Handle(_command, default);
        }

        // Assert
        await Assert.ThrowsAsync<DataConsistencyViolationException>(Act);
    }

    [Fact]
    public async Task ReturnCorrectErrorResultIfTransactionFails()
    {
        // Arrange
        var expectedError = new Error("expected error");
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(_account);
        useCaseBuilder.ConfigureConfirmationTokenRepository(confirmationToken);
        useCaseBuilder.ConfigureHasher(true);
        useCaseBuilder.ConfigureUnitOfWork(Result.Fail(expectedError));
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_command, default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(expectedError, result.Errors.First());
    }

    private class UseCaseBuilder
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IConfirmationTokenRepository> _confirmationTokenRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IHasher> _hasherMock = new();

        public ConfirmAccountEmailHandler Build()
        {
            return new ConfirmAccountEmailHandler(_confirmationTokenRepositoryMock.Object,
                _accountRepositoryMock.Object, _unitOfWorkMock.Object, _hasherMock.Object);
        }

        public void ConfigureAccountRepository(Account getByIdShouldReturn)
        {
            _accountRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>(), default)).ReturnsAsync(getByIdShouldReturn);
            _accountRepositoryMock.Setup(x => x.UpdateStatus(It.IsAny<Account>()));
        }

        public void ConfigureConfirmationTokenRepository(ConfirmationToken getConfirmationTokenShouldReturn)
        {
            _confirmationTokenRepositoryMock.Setup(x => x.Get(It.IsAny<Guid>()))
                .ReturnsAsync(getConfirmationTokenShouldReturn);
            _confirmationTokenRepositoryMock.Setup(x => x.Delete(It.IsAny<Guid>()));
        }

        public void ConfigureUnitOfWork(Result commitShouldReturn)
        {
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);
        }

        public void ConfigureHasher(bool verifyHashShouldReturn)
        {
            _hasherMock.Setup(x => x.VerifyHash(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(verifyHashShouldReturn);
        }
    }
}