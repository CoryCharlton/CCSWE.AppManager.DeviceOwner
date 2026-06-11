using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.Adb;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeviceServiceTests
{
    public class When_ListAsync_Is_Called
    {
        [Test]
        public async Task It_runs_adb_devices_l_at_the_resolved_path()
        {
            var adbLocator = new Mock<IAdbLocator>();
            adbLocator.SetupGet(locator => locator.AdbPath).Returns("/sdk/platform-tools/adb");

            var processRunner = new Mock<IProcessRunner>();
            processRunner
                .Setup(runner => runner.RunAsync("/sdk/platform-tools/adb", It.Is<IReadOnlyList<string>>(arguments => arguments.SequenceEqual(new[] { "devices", "-l" })), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessResult(0, "List of devices attached\nserial1\tdevice model:Pixel_7\n", string.Empty));

            var service = new DeviceService(processRunner.Object, adbLocator.Object, new LoggerFake<DeviceService>());

            var devices = await service.ListAsync();

            Assert.That(devices.Single().Serial, Is.EqualTo("serial1"));
            processRunner.VerifyAll();
        }

        [Test]
        public async Task It_returns_empty_when_adb_reports_no_devices()
        {
            var adbLocator = new Mock<IAdbLocator>();
            adbLocator.SetupGet(locator => locator.AdbPath).Returns("adb");

            var processRunner = new Mock<IProcessRunner>();
            processRunner
                .Setup(runner => runner.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessResult(0, "List of devices attached\n\n", string.Empty));

            var service = new DeviceService(processRunner.Object, adbLocator.Object, new LoggerFake<DeviceService>());

            var devices = await service.ListAsync();

            Assert.That(devices, Is.Empty);
        }
    }
}
