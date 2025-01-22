using Core.Application.DomainEventHandlers;
using Core.Domain.ConfirmationTokenAggregate.DomainEvents;
using Core.Ports.Kafka;
using Moq;
using Xunit;

namespace UnitTests.Core.Application.DomainEventHandlers;

public class ConfirmationTokenCreatedHandlerShould
{
    private readonly ConfirmationTokenCreatedDomainEvent _event = new(Guid.NewGuid(), "email@email.com");
    
    [Fact]
    public async Task CallPublishMethod()
    {
        // Arrange
        var messageBusMock = new Mock<IMessageBus>();
        var handler = new ConfirmationTokenCreatedHandler(messageBusMock.Object);

        // Act
        await handler.Handle(_event, CancellationToken.None);
        
        // Assert
        messageBusMock.Verify(x => x.Publish(_event, It.IsAny<CancellationToken>()), Times.Once);
    }
}