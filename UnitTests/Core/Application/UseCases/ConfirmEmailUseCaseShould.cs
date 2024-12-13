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
    private readonly ConfirmAccountEmailRequest _request = new(
        CorrelationId: Guid.NewGuid(), 
        AccountId: Guid.NewGuid(),
        ConfirmationToken: Guid.NewGuid());
    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
    
    [Fact]
    public async Task CanReturnSuccessResultForCorrectRequest()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));
        
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: _account);
        useCaseBuilder.ConfigureConfirmationTokenRepository(getConfirmationTokenShouldReturn: confirmationToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true);
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn: Result.Ok());
        var useCase = useCaseBuilder.Build();
    
        // Act
        var result = await useCase.Handle(_request, default);
    
        // Assert
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task CanReturnFailedResultIfGetConfirmationTokenReturnsNull()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));
        
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: _account);
        useCaseBuilder.ConfigureConfirmationTokenRepository(getConfirmationTokenShouldReturn: null);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true);
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn:Result.Ok());
        var useCase = useCaseBuilder.Build();
    
        // Act
        var result = await useCase.Handle(_request, default);
    
        // Assert
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task CanReturnFailedResultIfVerifyHashReturnsFalse()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(accountId: _account.Id, confirmationTokenHash: new string(c: '*', count: 60));
        
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: _account);
        useCaseBuilder.ConfigureConfirmationTokenRepository(getConfirmationTokenShouldReturn: confirmationToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: false);
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn: Result.Ok());
        var useCase = useCaseBuilder.Build();
    
        // Act
        var result = await useCase.Handle(_request, _: default);
    
        // Assert
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task CanThrowsDataConsistencyViolationExceptionIfGetByIdReturnsNull()
    {
        // Arrange
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));
        
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: null);
        useCaseBuilder.ConfigureConfirmationTokenRepository(getConfirmationTokenShouldReturn: confirmationToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true);
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn: Result.Ok());
        var useCase = useCaseBuilder.Build();
    
        // Act
        async Task Act() => await useCase.Handle(_request, default);
    
        // Assert
        await Assert.ThrowsAsync<DataConsistencyViolationException>(Act);
    }
    
    [Fact]
    public async Task CanReturnCorrectErrorResultIfTransactionFails()
    {
        // Arrange
        var expectedError = new Error("expected error");
        var confirmationToken = ConfirmationToken.Create(_account.Id, new string('*', 60));
        
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: _account);
        useCaseBuilder.ConfigureConfirmationTokenRepository(getConfirmationTokenShouldReturn: confirmationToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true);
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn: Result.Fail(expectedError));
        var useCase = useCaseBuilder.Build();
    
        // Act
        var result = await useCase.Handle(_request, default);
    
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

        public ConfirmAccountEmailHandler Build() => new(_confirmationTokenRepositoryMock.Object,
            _accountRepositoryMock.Object, _unitOfWorkMock.Object, _hasherMock.Object);

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

        public void ConfigureUnitOfWork(Result commitShouldReturn) =>
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);

        public void ConfigureHasher(bool verifyHashShouldReturn) =>
            _hasherMock.Setup(x => x.VerifyHash(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(verifyHashShouldReturn);
    }
}