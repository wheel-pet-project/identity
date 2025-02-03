using Core.Application.UseCases.Authenticate;
using Core.Domain.AccountAggregate;
using Core.Domain.RefreshTokenAggregate;
using Core.Infrastructure.Interfaces.JwtProvider;
using Core.Infrastructure.Interfaces.PasswordHasher;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.UseCases;

public class AuthenticateUseCaseShould
{
    private readonly AuthenticateAccountRequest _request = new(Guid.NewGuid(), "test@test.com", "somepassword");
    private const string ExpectedJwtAccessToken = "jwt_access_token";
    private const string ExpectedJwtRefreshToken = "jwt_refresh_token";
    
    [Fact]
    public async Task ReturnSuccessResultForCorrectRequest()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
        account.SetStatus(Status.PendingApproval);

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: account);
        useCaseBuilder.ConfigureRefreshTokenRepositoryMock();
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        useCaseBuilder.ConfigureJwtProvider(generateJwtAccessTokenShouldReturn: ExpectedJwtAccessToken, 
            generateJwtRefreshTokenShouldReturn: ExpectedJwtRefreshToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true);
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(
            new AuthenticateAccountResponse(ExpectedJwtAccessToken, ExpectedJwtRefreshToken), result.Value);
    }

    [Fact]
    public async Task ReturnFailedResultIfGetByEmailReturnNull()
    {
        // Arrange
        var expectedError = new Error("expected error");

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: null);
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ReturnFailedResultIfAccountCannotAuthenticate()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60)); 
        //аккаунт со статусом 'pending confirmation' не может быть авторизован и аунтентифицирован

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: account);
        useCaseBuilder.ConfigureRefreshTokenRepositoryMock();
        useCaseBuilder.ConfigureUnitOfWork(Result.Ok());
        useCaseBuilder.ConfigureJwtProvider(generateJwtAccessTokenShouldReturn: ExpectedJwtAccessToken, 
            generateJwtRefreshTokenShouldReturn: ExpectedJwtRefreshToken);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true);
        var useCase = useCaseBuilder.Build();
        
        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ReturnFailedResultIfPasswordInvalid()
    {
        // Arrange
        var account = Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
        account.SetStatus(Status.PendingApproval);

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: account);
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: false);
        var useCase = useCaseBuilder.Build();
        
        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ReturnErrorIfTransactionFails()
    {
        // Arrange
        var expectedError = new Error("expected error");
        var account = Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
        account.SetStatus(Status.PendingApproval);

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByEmailShouldReturn: account);
        useCaseBuilder.ConfigureRefreshTokenRepositoryMock();
        useCaseBuilder.ConfigureUnitOfWork(Result.Fail(expectedError));
        useCaseBuilder.ConfigureHasher(verifyHashShouldReturn: true);
        useCaseBuilder.ConfigureJwtProvider(generateJwtAccessTokenShouldReturn: ExpectedJwtAccessToken,
            generateJwtRefreshTokenShouldReturn: ExpectedJwtRefreshToken);
        var useCase = useCaseBuilder.Build();
        
        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equivalent(expectedError, result.Errors.First());
    }
    
    private class UseCaseBuilder
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IJwtProvider> _jwtProviderMock = new();
        private readonly Mock<IHasher> _hasherMock = new();
        private readonly TimeProvider _timeProvider = TimeProvider.System;

        public AuthenticateAccountHandler Build() =>
            new(_refreshTokenRepositoryMock.Object, _accountRepositoryMock.Object, _jwtProviderMock.Object,
                _unitOfWorkMock.Object, _hasherMock.Object, _timeProvider);

        public void ConfigureAccountRepository(Account getByEmailShouldReturn) =>
            _accountRepositoryMock.Setup(x => x.GetByEmail(It.IsAny<string>(), default))
                .ReturnsAsync(getByEmailShouldReturn);

        public void ConfigureRefreshTokenRepositoryMock() =>
            _refreshTokenRepositoryMock.Setup(x => x.Add(It.IsAny<RefreshToken>()));

        public void ConfigureUnitOfWork(Result commitShouldReturn) => 
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);

        public void ConfigureJwtProvider(string generateJwtAccessTokenShouldReturn, 
            string generateJwtRefreshTokenShouldReturn)
        {
            _jwtProviderMock.Setup(x => x.GenerateJwtAccessToken(It.IsAny<Account>()))
                .Returns(generateJwtAccessTokenShouldReturn);
            _jwtProviderMock.Setup(x => x.GenerateJwtRefreshToken(It.IsAny<Guid>()))
                .Returns(generateJwtRefreshTokenShouldReturn);
        }

        public void ConfigureHasher(bool verifyHashShouldReturn) =>
            _hasherMock.Setup(x => x.VerifyHash(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(verifyHashShouldReturn);
    }
}