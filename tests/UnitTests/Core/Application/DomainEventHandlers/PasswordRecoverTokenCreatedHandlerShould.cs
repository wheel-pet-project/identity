using Core.Application.DomainEventHandlers;
using Core.Domain.PasswordRecoverTokenAggregate.DomainEvents;
using Core.Ports.Kafka;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.DomainEventHandlers;

public class PasswordRecoverTokenCreatedHandlerShould
{
    private readonly PasswordRecoverTokenCreatedDomainEvent _event = new(Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task CallPublishMethod()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var handler = new PasswordRecoverTokenCreatedHandler(messageBusMock.Object);

        // Act
        await handler.Handle(_event, CancellationToken.None);

        // Assert
        messageBusMock.Verify(x => x.Publish(_event, It.IsAny<CancellationToken>()), Times.Once);
    }
}