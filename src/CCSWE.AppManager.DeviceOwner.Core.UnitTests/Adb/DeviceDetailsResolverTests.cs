using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.Adb;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeviceDetailsResolverTests
{
    private const string PixelProps = "ro.build.characteristics=nosdcard\nro.product.model=Pixel 8 Pro\n";

    private static DeviceDetailsResolver Create(IProcessRunner runner)
    {
        var adbLocator = new Mock<IAdbLocator>();
        adbLocator.SetupGet(locator => locator.AdbPath).Returns("adb");
        return new DeviceDetailsResolver(runner, adbLocator.Object);
    }

    public class When_ResolveAsync_Is_Called : DeviceDetailsResolverTests
    {
        [Test]
        public async Task It_resolves_and_caches_by_serial()
        {
            var runner = new Mock<IProcessRunner>();
            runner.Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessResult(0, PixelProps, string.Empty));

            var resolver = Create(runner.Object);

            var first = await resolver.ResolveAsync("serial1");
            var second = await resolver.ResolveAsync("serial1");

            Assert.That(first?.Name, Is.EqualTo("Pixel 8 Pro"));
            Assert.That(second?.Name, Is.EqualTo("Pixel 8 Pro"));
            runner.Verify(r => r.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task It_does_not_cache_a_transient_failure()
        {
            var runner = new Mock<IProcessRunner>();
            runner.SetupSequence(r => r.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessResult(1, string.Empty, "error: device offline"))
                .ReturnsAsync(new ProcessResult(0, PixelProps, string.Empty));

            var resolver = Create(runner.Object);

            var first = await resolver.ResolveAsync("serial1");
            var second = await resolver.ResolveAsync("serial1");

            Assert.That(first, Is.Null);
            Assert.That(second?.Name, Is.EqualTo("Pixel 8 Pro"));
            runner.Verify(r => r.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Test]
        public async Task It_returns_null_when_adb_cannot_launch()
        {
            var runner = new Mock<IProcessRunner>();
            runner.Setup(r => r.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ProcessLaunchException("adb", new InvalidOperationException()));

            Assert.That(await Create(runner.Object).ResolveAsync("serial1"), Is.Null);
        }
    }
}
