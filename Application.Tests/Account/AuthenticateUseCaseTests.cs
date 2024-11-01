using Application.Application.UseCases.Account.Authenticate;
using Application.Infrastructure.Interfaces.JwtProvider;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Domain.AccountAggregate;
using Moq;
using Xunit;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.Tests.Account;

public class AuthenticateUseCaseTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IHasher> _hasherMock;
    private readonly Domain.AccountAggregate.Account _testAccount;

    public AuthenticateUseCaseTests()
    {
        var factory = new AccountFactory();
        _testAccount = factory.CreateAccount(Role.Customer, Status.PendingApproval, 
            "email@mail.com", "+79008007060", "passwordHash");
        
        _accountRepositoryMock = new Mock<IAccountRepository>();
        
        _jwtProviderMock = new Mock<IJwtProvider>();
        _jwtProviderMock.Setup(m => m.GenerateAccessToken(
            It.IsAny<Domain.AccountAggregate.Account>())).Returns("token");
        
        _hasherMock = new Mock<IHasher>();
        _hasherMock.Setup(m => m.VerifyHash("password", "passwordHash")).Returns(true);
    }

    // [Fact]
    // public async Task Execute_Should_Return_Tokens_For_Valid_Account_And_Successfully_Save_Refresh_Token()
    // {
    //     // Arrange
    //     var request = new AuthenticateAccountRequest("email@mail.com", "password");
    //     
    //     _accountRepositoryMock.Setup(m => m.GetByEmail("email@mail.com", 
    //             default)).ReturnsAsync(_testAccount);
    //     _accountRepositoryMock.Setup(m => m.CreateAccount(It.IsAny<Domain.AccountAggregate.Account>(), 
    //             It.IsAny<Guid>())).ReturnsAsync(true);
    //     _accountRepositoryMock.Setup(m => m.CreateRefreshToken(
    //         It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>())).ReturnsAsync(true);
    //     
    //     var useCase = new AuthenticateAccountUseCase(
    //         _accountRepositoryMock.Object,
    //         _jwtProviderMock.Object,
    //         _hasherMock.Object);
    //
    //     // Act
    //     var actual = await useCase.Execute(request);
    //
    //     // Assert
    //     Assert.Equal("token", actual.Value.AccessToken);
    //     Assert.NotEqual(string.Empty, actual.Value.AccessToken);
    // }
    //
    // [Fact]
    // public async Task Execute_Should_Throw_ApplicationException_If_Account_Does_Not_Exist()
    // {
    //     // Arrange
    //     var request = new AuthenticateAccountRequest("email@mail.com", "password");
    //     
    //     Domain.AccountAggregate.Account nullReturns = null;
    //     _accountRepositoryMock.Setup(m => m.GetByEmail("email@mail.com", 
    //         default)).ReturnsAsync(nullReturns);
    //     
    //     var useCase = new AuthenticateAccountUseCase(
    //         _accountRepositoryMock.Object,
    //         _jwtProviderMock.Object,
    //         _hasherMock.Object);
    //
    //     // Act
    //     var act = () => useCase.Execute(request);
    //
    //     // Assert
    //     await Assert.ThrowsAsync<ApplicationException>(act);
    // }
    //
    // [Fact]
    // public async Task Execute_Should_Throw_ApplicationException_If_Account_Status_Is_PendingConfirmation()
    // {
    //     // Arrange
    //     var request = new AuthenticateAccountRequest("email@mail.com", "password");
    //
    //     _testAccount.SetStatus(Status.PendingConfirmation);
    //     _accountRepositoryMock.Setup(m => m.GetByEmail("email@mail.com", 
    //         default)).ReturnsAsync(_testAccount);
    //     _accountRepositoryMock.Setup(m => m.CreateAccount(It.IsAny<Domain.AccountAggregate.Account>(), 
    //         It.IsAny<Guid>())).ReturnsAsync(true);
    //     
    //     var useCase = new AuthenticateAccountUseCase(
    //         _accountRepositoryMock.Object,
    //         _jwtProviderMock.Object,
    //         _hasherMock.Object);
    //
    //     // Act
    //     var act = () => useCase.Execute(request);
    //
    //     // Assert
    //     await Assert.ThrowsAsync<ApplicationException>(act);
    // }
}