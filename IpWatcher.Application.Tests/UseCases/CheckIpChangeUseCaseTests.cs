using IpWatcher.Application.Abstractions;
using IpWatcher.Application.UseCases;
using IpWatcher.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace IpWatcher.Application.Tests.UseCases;

public sealed class CheckIpChangeUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_WhenNoPreviousIp_NotifiesAndSaves()
    {
        // Arrange
        var ct = CancellationToken.None;
        var currentIp = IpAddress.Parse("1.1.1.1");

        var publicIpProvider = new Mock<IPublicIpProvider>(MockBehavior.Strict);
        var ipStorage = new Mock<IIpStorage>(MockBehavior.Strict);
        var emailNotifier = new Mock<IEmailNotifier>(MockBehavior.Strict);

        publicIpProvider
            .Setup(x => x.GetPublicIpAsync(ct))
            .ReturnsAsync(currentIp);

        ipStorage
            .Setup(x => x.LoadLastIpAsync(ct))
            .ReturnsAsync((IpAddress?)null);

        var sequence = new MockSequence();
        emailNotifier
            .InSequence(sequence)
            .Setup(x => x.NotifyIpChangedAsync(null, currentIp, ct))
            .Returns(Task.CompletedTask);

        ipStorage
            .InSequence(sequence)
            .Setup(x => x.SaveLastIpAsync(currentIp, ct))
            .Returns(Task.CompletedTask);

        var useCase = new CheckIpChangeUseCase(
            publicIpProvider.Object,
            ipStorage.Object,
            emailNotifier.Object,
            NullLogger<CheckIpChangeUseCase>.Instance);

        // Act
        await useCase.ExecuteAsync(ct);

        // Asert
        publicIpProvider.Verify(x => x.GetPublicIpAsync(ct), Times.Once);
        ipStorage.Verify(x => x.LoadLastIpAsync(ct), Times.Once);
        emailNotifier.Verify(x => x.NotifyIpChangedAsync(null, currentIp, ct), Times.Once);
        ipStorage.Verify(x => x.SaveLastIpAsync(currentIp, ct), Times.Once);

        publicIpProvider.VerifyNoOtherCalls();
        ipStorage.VerifyNoOtherCalls();
        emailNotifier.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenIpUnchanged_DoesNotNotifyOrSave()
    {
        // Arrange
        var ct = CancellationToken.None;
        var currentIp = IpAddress.Parse("1.1.1.1");
        var previousIp = IpAddress.Parse("1.1.1.1"); // equal by Value

        var publicIpProvider = new Mock<IPublicIpProvider>(MockBehavior.Strict);
        var ipStorage = new Mock<IIpStorage>(MockBehavior.Strict);
        var emailNotifier = new Mock<IEmailNotifier>(MockBehavior.Strict);

        publicIpProvider
            .Setup(x => x.GetPublicIpAsync(ct))
            .ReturnsAsync(currentIp);

        ipStorage
            .Setup(x => x.LoadLastIpAsync(ct))
            .ReturnsAsync(previousIp);

        // Strict mocks ensure Notify/Save would fail the test if invoked.
        var useCase = new CheckIpChangeUseCase(
            publicIpProvider.Object,
            ipStorage.Object,
            emailNotifier.Object,
            NullLogger<CheckIpChangeUseCase>.Instance);

        // Act
        await useCase.ExecuteAsync(ct);

        // Asert
        publicIpProvider.Verify(x => x.GetPublicIpAsync(ct), Times.Once);
        ipStorage.Verify(x => x.LoadLastIpAsync(ct), Times.Once);

        emailNotifier.VerifyNoOtherCalls();
        ipStorage.Verify(x => x.SaveLastIpAsync(It.IsAny<IpAddress>(), It.IsAny<CancellationToken>()), Times.Never);

        publicIpProvider.VerifyNoOtherCalls();
        ipStorage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_WhenIpChanged_NotifiesAndSaves()
    {
        // Arrange
        var ct = CancellationToken.None;
        var previousIp = IpAddress.Parse("1.1.1.1");
        var currentIp = IpAddress.Parse("2.2.2.2");

        var publicIpProvider = new Mock<IPublicIpProvider>(MockBehavior.Strict);
        var ipStorage = new Mock<IIpStorage>(MockBehavior.Strict);
        var emailNotifier = new Mock<IEmailNotifier>(MockBehavior.Strict);

        publicIpProvider
            .Setup(x => x.GetPublicIpAsync(ct))
            .ReturnsAsync(currentIp);

        ipStorage
            .Setup(x => x.LoadLastIpAsync(ct))
            .ReturnsAsync(previousIp);

        var sequence = new MockSequence();
        emailNotifier
            .InSequence(sequence)
            .Setup(x => x.NotifyIpChangedAsync(previousIp, currentIp, ct))
            .Returns(Task.CompletedTask);

        ipStorage
            .InSequence(sequence)
            .Setup(x => x.SaveLastIpAsync(currentIp, ct))
            .Returns(Task.CompletedTask);

        var useCase = new CheckIpChangeUseCase(
            publicIpProvider.Object,
            ipStorage.Object,
            emailNotifier.Object,
            NullLogger<CheckIpChangeUseCase>.Instance);

        // Act
        await useCase.ExecuteAsync(ct);

        // Asert
        publicIpProvider.Verify(x => x.GetPublicIpAsync(ct), Times.Once);
        ipStorage.Verify(x => x.LoadLastIpAsync(ct), Times.Once);
        emailNotifier.Verify(x => x.NotifyIpChangedAsync(previousIp, currentIp, ct), Times.Once);
        ipStorage.Verify(x => x.SaveLastIpAsync(currentIp, ct), Times.Once);

        publicIpProvider.VerifyNoOtherCalls();
        ipStorage.VerifyNoOtherCalls();
        emailNotifier.VerifyNoOtherCalls();
    }
}