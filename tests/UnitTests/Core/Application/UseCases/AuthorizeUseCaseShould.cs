using Core.Application.UseCases.Authorize;
using Core.Infrastructure.Interfaces.JwtProvider;
using FluentResults;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.UseCases;

public class AuthorizeUseCaseShould
{
    private readonly AuthorizeAccountCommand _command = new("jwt_access_token");

    [Fact]
    public async Task ReturnSuccessResultForCorrectRequest()
    {
        // Arrange
        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureJwtProvider(Result.Ok());
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_command, default);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ReturnCorrectErrorIfAccessTokenIsInvalid()
    {
        // Arrange
        var expectedError = new Error("expected error");

        var useCaseBuilder = new UseCaseBuilder();
        useCaseBuilder.ConfigureJwtProvider(Result.Fail(expectedError));
        var useCase = useCaseBuilder.Build();

        // Act
        var result = await useCase.Handle(_command, default);

        // Assert
        Assert.True(result.IsFailed);
        Assert.Equivalent(expectedError, result.Errors.First());
    }

    private class UseCaseBuilder
    {
        private readonly Mock<IJwtProvider> _jwtProviderMock = new();

        public AuthorizeAccountHandler Build()
        {
            return new AuthorizeAccountHandler(_jwtProviderMock.Object);
        }

        public void ConfigureJwtProvider(Result verifyJwtAccessTokenShouldReturn)
        {
            _jwtProviderMock.Setup(x => x.VerifyJwtAccessToken(It.IsAny<string>()))
                .ReturnsAsync(verifyJwtAccessTokenShouldReturn);
        }
    }
}