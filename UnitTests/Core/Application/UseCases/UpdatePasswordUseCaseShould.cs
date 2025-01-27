using Core.Application.UseCases.UpdatePassword;
using Core.Domain.AccountAggregate;
using Core.Domain.PasswordRecoverTokenAggregate;
using Core.Domain.Services.UpdateAccountPasswordService;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using MediatR;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.UseCases;

public class UpdatePasswordUseCaseShould
{
    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
    private readonly UpdateAccountPasswordRequest _request = new(Guid.NewGuid(), "newpassword", "test@test.com",
        Guid.NewGuid());
    
    [Fact]
    public async Task ReturnSuccessForCorrectRequest()
    {
        // Arrange
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, new string('h', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: _account);
        useCaseBuilder.ConfigurePasswordRecoverTokenRepository(
            getPasswordRecoverTokenShouldReturn: passwordRecoverToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true, generateHashShouldReturn: new string('*', 60));
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);
        
        // Assert
        Assert.True(result.IsSuccess);
    }
    
    [Fact]
    public async Task ReturnCorrectErrorIfGetPasswordRecoverTokenReturnsNull()
    {
        // Arrange
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: _account);
        useCaseBuilder.ConfigurePasswordRecoverTokenRepository(
            getPasswordRecoverTokenShouldReturn: null);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true, generateHashShouldReturn: new string('*', 60));
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);
        
        // Assert
        Assert.True(result.IsFailed);
    }
    
    [Fact]
    public async Task ReturnFailedResultErrorIfGetByEmailReturnsNull()
    {
        // Arrange
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, new string('h', 60));
        
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: null);
        useCaseBuilder.ConfigurePasswordRecoverTokenRepository(
            getPasswordRecoverTokenShouldReturn: passwordRecoverToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true, generateHashShouldReturn: new string('*', 60));
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);
        
        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ReturnFailedResultErrorIfVerifyHashReturnsFalse()
    {
        // Arrange
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, new string('h', 60));
        
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: _account);
        useCaseBuilder.ConfigurePasswordRecoverTokenRepository(
            getPasswordRecoverTokenShouldReturn: passwordRecoverToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: false, generateHashShouldReturn: new string('*', 60));
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);
        
        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ReturnCorrectErrorIfTransactionFails()
    {
        // Arrange
        var expectedError = new Error("expected error");
        var passwordRecoverToken = PasswordRecoverToken.Create(_account, new string('h', 60));

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: _account);
        useCaseBuilder.ConfigurePasswordRecoverTokenRepository(
            getPasswordRecoverTokenShouldReturn: passwordRecoverToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true, generateHashShouldReturn: new string('*', 60));
        useCaseBuilder.ConfigureUnitOfWork(Result.Fail(expectedError));
        var useCase = useCaseBuilder.Build();
        
        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equivalent(expectedError, result.Errors.First());
    }

    private class UseCaseBuilder
    {
        private readonly Mock<IUpdateAccountPasswordService> _updateAccountPasswordServiceMock = new();
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IPasswordRecoverTokenRepository> _passwordRecoverTokenRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IHasher> _hasherMock = new();
        private readonly Mock<IMediator> _mediatorMock = new();

        public UpdateAccountPasswordHandler Build()
        {
            ConfigureUpdateAccountPasswordService(Result.Ok());
            
            return new UpdateAccountPasswordHandler(
                _updateAccountPasswordServiceMock.Object,
                _passwordRecoverTokenRepositoryMock.Object,
                _accountRepositoryMock.Object,
                _unitOfWorkMock.Object,
                _hasherMock.Object,
                _mediatorMock.Object);
        }

        public void ConfigureUpdateAccountPasswordService(Result updateAccountPasswordShouldReturn) =>
            _updateAccountPasswordServiceMock.Setup(x => x.UpdatePassword(It.IsAny<Account>(), It.IsAny<string>()))
                .Returns(Result.Ok);
        
        public void ConfigureAccountRepository(Account getByEmailShouldReturn)
        {
            _accountRepositoryMock.Setup(x => x.GetByEmail(It.IsAny<string>(), default))
                .ReturnsAsync(getByEmailShouldReturn);
            _accountRepositoryMock.Setup(x => x.UpdatePasswordHash(It.IsAny<Account>()));
        }

        public void ConfigurePasswordRecoverTokenRepository(PasswordRecoverToken getPasswordRecoverTokenShouldReturn)
        {
            _passwordRecoverTokenRepositoryMock.Setup(x => x.Get(It.IsAny<Guid>()))
                .ReturnsAsync(getPasswordRecoverTokenShouldReturn);
            _passwordRecoverTokenRepositoryMock.Setup(x => x.UpdateAppliedStatus(It.IsAny<PasswordRecoverToken>()));
        }

        public void ConfigureUnitOfWork(Result commitShouldReturn) =>
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);

        public void ConfigureHasher(bool verifyHashShouldReturn, string generateHashShouldReturn)
        {
            _hasherMock.Setup(x => x.VerifyHash(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(verifyHashShouldReturn);
            _hasherMock.Setup(x => x.GenerateHash(It.IsAny<string>())).Returns(generateHashShouldReturn);
        }
    }
}