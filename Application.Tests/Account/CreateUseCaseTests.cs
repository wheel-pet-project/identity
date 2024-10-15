using Application.Infrastructure.Interfaces.PasswordHasher;
using Application.Infrastructure.Interfaces.Repositories;
using Application.UseCases.Account.Create;
using Domain.AccountAggregate;
using JetBrains.Annotations;
using Moq;
using Xunit;

namespace Application.Tests.Account;

[TestSubject(typeof(CreateAccountUseCase))]
public class CreateUseCaseTests
{
    private readonly CreateAccountUseCase _useCase;
    
    public CreateUseCaseTests()
    {
         var accountRepositoryMock = new Mock<IAccountRepository>();
         var hasherMock = new Mock<IHasher>();
         hasherMock.Setup(m => m.GenerateHash(It.IsAny<string>())).Returns("passwordHash");
         _useCase = new CreateAccountUseCase(accountRepositoryMock.Object, hasherMock.Object);
    }
    
    [Fact]
    public async Task Execute_Should_Return_Guid_For_Created_Account()
    {
        // Arrange
        var request = new CreateAccountRequest(Guid.NewGuid(), Role.Customer, 
            "test@test.com", "+79008007060", "password");

        // Act
        var actual = await _useCase.Execute(request);

        // Assert
        Assert.IsType<Guid>(actual.AccountId);
    }
    
}