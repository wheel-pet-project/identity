using Core.Application.UseCases.RefreshAccessToken;
using Core.Domain.AccountAggregate;
using Core.Domain.RefreshTokenAggregate;
using Core.Domain.SharedKernel.Exceptions.DataConsistencyViolationException;
using Core.Infrastructure.Interfaces.JwtProvider;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.UseCases;

public class RefreshAccessTokenUseCaseShould
{
    private readonly RefreshAccountAccessTokenRequest _request = new(Guid.NewGuid(), "jwt_refresh_token");
    private const string ExpectedJwtAccessToken = "new_jwt_access_token";
    private const string ExpectedJwtRefreshToken = "new_jwt_refresh_token";
    private readonly Account _account =
        Account.Create(Role.Customer, "test@test.com", "+79008007060", new string('*', 60));
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    
    
    [Fact]
    public async Task ReturnSuccessForCorrectRequest()
    {
        // Arrange
        _account.SetStatus(Status.PendingApproval);
        var refreshToken = RefreshToken.Create(_account, _timeProvider);

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: _account);
        useCaseBuilder.ConfigureRefreshTokenRepository(getRefreshTokenInfoShouldReturn: refreshToken);
        useCaseBuilder.ConfigureJwtProvider(verifyJwtRefreshTokenShouldReturn: Result.Ok(It.IsAny<Guid>()));
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn: Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equivalent(new RefreshAccountAccessTokenResponse(ExpectedJwtAccessToken, ExpectedJwtRefreshToken),
            result.Value);
    }

    [Fact]
    public async Task ReturnCorrectErrorIfVerifyingJwtRefreshTokenFails()
    {
        // Arrange
        _account.SetStatus(Status.PendingApproval);
        var expectedError = new Error("expected error");

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureJwtProvider(verifyJwtRefreshTokenShouldReturn: Result.Fail(expectedError));
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equal(expectedError, result.Errors.First());
    }

    [Fact]
    public async Task ReturnFailedResultIfGetRefreshTokenReturnsNull()
    {
        // Arrange
        _account.SetStatus(Status.PendingApproval);
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureJwtProvider(verifyJwtRefreshTokenShouldReturn: Result.Ok(It.IsAny<Guid>()));
        useCaseBuilder.ConfigureRefreshTokenRepository(getRefreshTokenInfoShouldReturn: null);
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task ThrowsDataConsistencyViolationExceptionIfGetByIdReturnNotFoundError()
    {
        // Arrange
        _account.SetStatus(Status.PendingApproval);
        var refreshToken = RefreshToken.Create(_account, _timeProvider);

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: null);
        useCaseBuilder.ConfigureRefreshTokenRepository(getRefreshTokenInfoShouldReturn: refreshToken);
        useCaseBuilder.ConfigureJwtProvider(verifyJwtRefreshTokenShouldReturn: Result.Ok(It.IsAny<Guid>()));
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn: Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        async Task Act() => await useCase.Handle(_request, default);

        // Assert
        await Assert.ThrowsAsync<DataConsistencyViolationException>(Act);
    }

    [Fact]
    public async Task ReturnFailedResultIfAccountCannotBeAuthenticated()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account, _timeProvider);

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: _account);
        useCaseBuilder.ConfigureRefreshTokenRepository(getRefreshTokenInfoShouldReturn: refreshToken);
        useCaseBuilder.ConfigureJwtProvider(verifyJwtRefreshTokenShouldReturn: Result.Ok(It.IsAny<Guid>()));
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn: Result.Ok());
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
        _account.SetStatus(Status.PendingApproval);
        var expectedError = new Error("expected error");
        var refreshToken = RefreshToken.Create(_account, _timeProvider);

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureAccountRepository(getByIdShouldReturn: _account);
        useCaseBuilder.ConfigureRefreshTokenRepository(getRefreshTokenInfoShouldReturn: refreshToken);
        useCaseBuilder.ConfigureJwtProvider(verifyJwtRefreshTokenShouldReturn: Result.Ok(refreshToken.Id),
            generateJwtAccessTokenShouldReturn: ExpectedJwtAccessToken,
            generateJwtRefreshTokenShouldReturn: ExpectedJwtRefreshToken);
        useCaseBuilder.ConfigureUnitOfWork(commitShouldReturn: Result.Fail(expectedError));
        var useCase = useCaseBuilder.Build();
        
        // Act
        var result = await useCase.Handle(_request, default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equivalent(expectedError, result.Errors.First());
    }

    private class UseCaseBuilder
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly Mock<IJwtProvider> _jwtProviderMock = new();
        private readonly TimeProvider _timeProvider = TimeProvider.System;

        public RefreshAccountAccessTokenHandler Build() =>
            new(_accountRepositoryMock.Object, _refreshTokenRepositoryMock.Object, _jwtProviderMock.Object,
                _unitOfWorkMock.Object, _timeProvider);

        public void ConfigureAccountRepository(Account getByIdShouldReturn) => 
            _accountRepositoryMock.Setup(x => x.GetById(It.IsAny<Guid>(), default)).ReturnsAsync(getByIdShouldReturn);

        public void ConfigureRefreshTokenRepository(RefreshToken getRefreshTokenInfoShouldReturn)
        {
            _refreshTokenRepositoryMock.Setup(x => x.GetNotRevokedToken(It.IsAny<Guid>()))
                .ReturnsAsync(getRefreshTokenInfoShouldReturn);
            _refreshTokenRepositoryMock
                .Setup(x => x.AddTokenAndRevokeOldToken(It.IsAny<RefreshToken>(), It.IsAny<RefreshToken>()));
        }

        public void ConfigureUnitOfWork(Result commitShouldReturn) =>
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);

        public void ConfigureJwtProvider(Result<Guid> verifyJwtRefreshTokenShouldReturn,
            string generateJwtAccessTokenShouldReturn = ExpectedJwtAccessToken, 
            string generateJwtRefreshTokenShouldReturn = ExpectedJwtRefreshToken)
        {
            _jwtProviderMock.Setup(x => x.VerifyJwtRefreshToken(It.IsAny<string>()))
                .ReturnsAsync(verifyJwtRefreshTokenShouldReturn);
            _jwtProviderMock.Setup(x => x.GenerateJwtAccessToken(It.IsAny<Account>()))
                .Returns(generateJwtAccessTokenShouldReturn);
            _jwtProviderMock.Setup(x => x.GenerateJwtRefreshToken(It.IsAny<Guid>()))
                .Returns(generateJwtRefreshTokenShouldReturn);
        }
    }
}