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
    
    
    [Fact]
    public async Task CanReturnSuccessForCorrectRequest()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);

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
    public async Task CanReturnCorrectErrorIfVerifyingJwtRefreshTokenFails()
    {
        // Arrange
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
    public async Task CanReturnFailedResultIfGetRefreshTokenReturnsNull()
    {
        // Arrange
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
    public async Task CanThrowsDataConsistencyViolationExceptionIfGetByIdReturnNotFoundError()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account);

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
    public async Task CanReturnCorrectErrorIfTransactionFails()
    {
        // Arrange
        var expectedError = new Error("expected error");
        var refreshToken = RefreshToken.Create(_account);

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

        public RefreshAccountAccessTokenHandler Build() =>
            new(_accountRepositoryMock.Object, _refreshTokenRepositoryMock.Object, _jwtProviderMock.Object,
                _unitOfWorkMock.Object);

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