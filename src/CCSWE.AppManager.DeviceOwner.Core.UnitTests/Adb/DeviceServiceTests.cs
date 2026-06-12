using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.Adb;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeviceServiceTests
{
    private static IDeviceDetailsResolver DetailsResolver(DeviceDetails? details = null)
    {
        var resolver = new Mock<IDeviceDetailsResolver>();
        resolver.Setup(r => r.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(details);
        return resolver.Object;
    }

    public class When_ListAsync_Is_Called : DeviceServiceTests
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

            var service = new DeviceService(processRunner.Object, adbLocator.Object, DetailsResolver(), new LoggerFake<DeviceService>());

            var devices = await service.ListAsync();

            Assert.That(devices.Single().Serial, Is.EqualTo("serial1"));
            processRunner.VerifyAll();
        }

        [Test]
        public async Task It_applies_the_resolved_details_to_online_devices()
        {
            var adbLocator = new Mock<IAdbLocator>();
            adbLocator.SetupGet(locator => locator.AdbPath).Returns("adb");

            var processRunner = new Mock<IProcessRunner>();
            processRunner
                .Setup(runner => runner.RunAsync(It.IsAny<string>(), It.Is<IReadOnlyList<string>>(arguments => arguments.SequenceEqual(new[] { "devices", "-l" })), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessResult(0, "List of devices attached\nemulator-5554\tdevice model:sdk_gphone16k_x86_64\n", string.Empty));

            var service = new DeviceService(processRunner.Object, adbLocator.Object, DetailsResolver(new DeviceDetails("Pixel 9", DeviceForm.Phone, true)), new LoggerFake<DeviceService>());

            var device = (await service.ListAsync()).Single();

            Assert.That(device.Name, Is.EqualTo("Pixel 9"));
            Assert.That(device.IsEmulator, Is.True);
            Assert.That(device.Form, Is.EqualTo(DeviceForm.Phone));
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

            var service = new DeviceService(processRunner.Object, adbLocator.Object, DetailsResolver(), new LoggerFake<DeviceService>());

            var devices = await service.ListAsync();

            Assert.That(devices, Is.Empty);
        }
    }
}
