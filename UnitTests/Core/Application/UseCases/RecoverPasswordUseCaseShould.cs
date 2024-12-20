using Core.Application.UseCases.RecoverPassword;
using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.UseCases;

public class RecoverPasswordUseCaseShould
{
    private readonly RecoverAccountPasswordRequest _request = new(Guid.NewGuid(), "email@email.com");
    
    [Fact]
    public async Task CanReturnSuccessResultForCorrectRequest()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: account);
        useCaseBuilder.ConfigureHasherShouldReturn(generateHashShouldReturn: new string('*', 60));
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task CanReturnFailedResultIfGetByEmailFails()
    {
        // Arrange
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: null);
        var useCase = useCaseBuilder.Build();
        
        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task CanReturnCorrectErrorIfTransactionFails()
    {
        // Arrange
        var expectedError = new Error("expected error message");
        var account = Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: account);
        useCaseBuilder.ConfigureHasherShouldReturn(generateHashShouldReturn: new string('*', 60));
        useCaseBuilder.ConfigureUnitOfWork(Result.Fail(expectedError));
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
        private readonly Mock<IPasswordRecoverTokenRepository> _passwordRecoverTokenRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IOutbox> _outboxMock = new();
        private readonly Mock<IHasher> _hasher = new();

        public RecoverAccountPasswordHandler Build() =>
            new(_passwordRecoverTokenRepositoryMock.Object, _accountRepositoryMock.Object, _unitOfWorkMock.Object, 
                _outboxMock.Object, _hasher.Object);
        
        public void ConfigureAccountRepository(Account getByEmailShouldReturn) =>
            _accountRepositoryMock.Setup(x => x.GetByEmail(It.IsAny<string>(), default))
                .ReturnsAsync(getByEmailShouldReturn);

        // public void ConfigurePasswordRecoverRepository() =>
        //     _passwordRecoverTokenRepositoryMock
        //         .Setup(x => x.Add(It.IsAny<PasswordRecoverToken>()));

        public void ConfigureUnitOfWork(Result commitShouldReturn) =>
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);
        
        public void ConfigureHasherShouldReturn(string generateHashShouldReturn) => 
            _hasher.Setup(x => x.GenerateHash(It.IsAny<string>())).Returns(generateHashShouldReturn);
    }
}