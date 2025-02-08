using Core.Application.DomainEventHandlers;
using Core.Domain.AccountAggregate;
using Core.Domain.AccountAggregate.DomainEvents;
using Core.Domain.RefreshTokenAggregate;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.DomainEventHandlers;

public class AccountPasswordUpdatedHandlerShould
{
    private readonly Account _account =
        Account.Create(Role.Customer, "email@mail.com", "+79008007060", new string('*', 60), Guid.NewGuid());
    private readonly TimeProvider _timeProvider = TimeProvider.System;
    
    [Fact]
    public async Task ChangeTokensStateToRevoke()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account, _timeProvider);
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureRefreshTokenRepository([refreshToken]);
        var handler = handlerBuilder.Build();

        // Act
        await handler.Handle(new AccountPasswordUpdatedDomainEvent(Guid.NewGuid()), default);

        // Assert
        Assert.True(refreshToken.IsRevoked);
    }
    
    private class HandlerBuilder
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public AccountPasswordUpdatedHandler Build() => new(_refreshTokenRepositoryMock.Object);

        public void ConfigureRefreshTokenRepository(List<RefreshToken> getNotRevokedTokensByAccountIdShouldReturn) =>
            _refreshTokenRepositoryMock.Setup(x => x.GetNotRevokedTokensByAccountId(It.IsAny<Guid>()))
                .ReturnsAsync(getNotRevokedTokensByAccountIdShouldReturn);
    }
}