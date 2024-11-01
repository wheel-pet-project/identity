using Application.Application.UseCases.Account.Create;
using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Domain.AccountAggregate;
using JetBrains.Annotations;
using Moq;
using Xunit;
using ApplicationException = Application.Exceptions.ApplicationException;

namespace Application.Tests.Account;

[TestSubject(typeof(CreateAccountUseCase))]
public class CreateUseCaseTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IHasher> _hasherMock;

    public CreateUseCaseTests()
    {
         _accountRepositoryMock = new Mock<IAccountRepository>();
         
         _hasherMock = new Mock<IHasher>();
         _hasherMock.Setup(m => m.GenerateHash(It.IsAny<string>())).Returns("passwordHash");
    }
    
    // [Fact]
    // public async Task Execute_Should_Return_Guid_For_Created_Account()
    // {
    //     // Arrange
    //     var request = new CreateAccountRequest(Guid.NewGuid(), Role.Customer, 
    //         "test@test.com", "+79008007060", "password");
    //     _accountRepositoryMock
    //         .Setup(m => m.CreateAccount(It.IsAny<Domain.AccountAggregate.Account>(),
    //             It.IsAny<Guid>())).ReturnsAsync(true);
    //     var useCase = new CreateAccountUseCase(_accountRepositoryMock.Object, _hasherMock.Object);
    //
    //     // Act
    //     var actual = await useCase.Execute(request);
    //
    //     // Assert
    //     Assert.IsType<Guid>(actual.Value.AccountId);
    // }
    //
    // [Fact]
    // public async Task Execute_Should_Throw_ApplicationException_If_Create_Account_Fails()
    // {
    //     // Arrange
    //     var request = new CreateAccountRequest(Guid.NewGuid(), Role.Customer, 
    //         "test@test.com", "+79008007060", "password");
    //     _accountRepositoryMock
    //         .Setup(m => m.CreateAccount(It.IsAny<Domain.AccountAggregate.Account>(),
    //             It.IsAny<Guid>())).ReturnsAsync(false);
    //     var useCase = new CreateAccountUseCase(_accountRepositoryMock.Object, _hasherMock.Object);
    //
    //     // Act
    //     var act = () => useCase.Execute(request);
    //
    //     // Assert
    //     await Assert.ThrowsAsync<ApplicationException>(act);
    // }
}