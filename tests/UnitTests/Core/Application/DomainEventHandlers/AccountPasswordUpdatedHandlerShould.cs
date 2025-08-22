using Core.Application.DomainEventHandlers;
using Core.Domain.AccountAggregate;
using Core.Domain.AccountAggregate.DomainEvents;
using Core.Domain.RefreshTokenAggregate;
using Core.Ports.Postgres;
using Core.Ports.Postgres.Repositories;
using FluentResults;
using Moq;
using Npgsql;
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
        handlerBuilder.ConfigureUnitOfWork(Result.Ok);
        var handler = handlerBuilder.Build();

        // Act
        await handler.Handle(new AccountPasswordUpdatedDomainEvent(Guid.NewGuid()), default);

        // Assert
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public async Task ThrowExceptionFromTransactionFailErrorIfCommitFailed()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(_account, _timeProvider);
        var handlerBuilder = new HandlerBuilder();
        handlerBuilder.ConfigureRefreshTokenRepository([refreshToken]);
        handlerBuilder.ConfigureUnitOfWork(() => throw new NpgsqlException());
        var handler = handlerBuilder.Build();

        // Act
        async Task Act() => await handler.Handle(new AccountPasswordUpdatedDomainEvent(Guid.NewGuid()), default);

        // Assert
        await Assert.ThrowsAsync<NpgsqlException>(Act);
    }

    private class HandlerBuilder
    {
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

        public AccountPasswordUpdatedHandler Build()
        {
            return new AccountPasswordUpdatedHandler(_refreshTokenRepositoryMock.Object, _unitOfWorkMock.Object);
        }

        public void ConfigureRefreshTokenRepository(List<RefreshToken> getNotRevokedTokensByAccountIdShouldReturn)
        {
            _refreshTokenRepositoryMock.Setup(x => x.GetNotRevokedTokensByAccountId(It.IsAny<Guid>()))
                .ReturnsAsync(getNotRevokedTokensByAccountIdShouldReturn);
        }

        public void ConfigureUnitOfWork(Func<Result> commitShouldReturn)
        {
            _unitOfWorkMock.Setup(x => x.Commit()).ReturnsAsync(commitShouldReturn);
        }
    }
}