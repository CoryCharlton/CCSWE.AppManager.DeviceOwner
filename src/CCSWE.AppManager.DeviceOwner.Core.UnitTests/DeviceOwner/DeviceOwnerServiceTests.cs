using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Core.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.DeviceOwner;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeviceOwnerServiceTests
{
    private static DeviceOwnerService CreateService(ProcessResult result, out Mock<IProcessRunner> processRunner)
    {
        var adbLocator = new Mock<IAdbLocator>();
        adbLocator.SetupGet(locator => locator.AdbPath).Returns("adb");

        processRunner = new Mock<IProcessRunner>();
        processRunner
            .Setup(runner => runner.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

        return new DeviceOwnerService(processRunner.Object, adbLocator.Object, new LoggerFake<DeviceOwnerService>());
    }

    public class When_SetAsync_Is_Called : DeviceOwnerServiceTests
    {
        [Test]
        public async Task It_fails_without_running_adb_when_the_serial_is_blank()
        {
            var service = CreateService(new ProcessResult(0, string.Empty, string.Empty), out var processRunner);

            var result = await service.SetAsync("  ");

            Assert.That(result.Success, Is.False);
            processRunner.Verify(runner => runner.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task It_targets_the_App_Manager_component_for_the_selected_serial()
        {
            var service = CreateService(new ProcessResult(0, "Success: Device owner set to package ComponentInfo{...}", string.Empty), out var processRunner);

            var result = await service.SetAsync("serial1");

            Assert.That(result.Success, Is.True);
            processRunner.Verify(runner => runner.RunAsync(
                "adb",
                It.Is<IReadOnlyList<string>>(arguments => arguments.SequenceEqual(new[]
                {
                    "-s", "serial1", "shell", "dpm", "set-device-owner",
                    "com.ccswe.appmanager.deviceowner/com.ccswe.appmanager.receivers.DeviceAdminReceiver",
                })),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task It_fails_on_a_nonzero_exit_code()
        {
            var service = CreateService(new ProcessResult(1, string.Empty, "error: no devices/emulators found"), out _);

            var result = await service.SetAsync("serial1");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo("error: no devices/emulators found"));
        }

        [Test]
        public async Task It_treats_a_zero_exit_with_an_exception_in_the_output_as_a_failure()
        {
            var output = "java.lang.IllegalStateException: Not allowed to set the device owner because there are already some accounts on the device";
            var service = CreateService(new ProcessResult(0, output, string.Empty), out _);

            var result = await service.SetAsync("serial1");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo(DeviceOwnerMessages.AccountsPresent));
        }

        [Test]
        public async Task It_returns_a_timeout_message_when_the_command_hangs()
        {
            var adbLocator = new Mock<IAdbLocator>();
            adbLocator.SetupGet(locator => locator.AdbPath).Returns("adb");

            var processRunner = new Mock<IProcessRunner>();
            processRunner
                .Setup(runner => runner.RunAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
                .Returns(async (string _, IReadOnlyList<string> _, CancellationToken cancellationToken) =>
                {
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                    return new ProcessResult(0, string.Empty, string.Empty);
                });

            var service = new DeviceOwnerService(processRunner.Object, adbLocator.Object, new LoggerFake<DeviceOwnerService>(), TimeSpan.FromMilliseconds(50));

            var result = await service.SetAsync("serial1");

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Is.EqualTo(DeviceOwnerMessages.Timeout));
        }
    }
}
