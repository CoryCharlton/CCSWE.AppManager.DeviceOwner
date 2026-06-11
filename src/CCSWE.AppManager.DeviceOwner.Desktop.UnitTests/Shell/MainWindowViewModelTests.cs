using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;
using CCSWE.AppManager.DeviceOwner.Desktop.Shell;
using CCSWE.AppManager.DeviceOwner.Desktop.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Desktop.UnitTests.Shell;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MainWindowViewModelTests
{
    private static AdbDevice Online(string serial, string? model = null) => new(serial, "device", model, null, null, null);

    public class When_RefreshAsync_Is_Called : MainWindowViewModelTests
    {
        [Test]
        public async Task It_lists_only_online_devices_and_selects_the_first()
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.Setup(s => s.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new AdbDevice[]
            {
                Online("serial1", "Pixel 7"),
                new("serial2", "unauthorized", null, null, null, null),
            });

            var viewModel = new MainWindowViewModel(deviceService.Object, Mock.Of<IDeviceOwnerService>(), new FakeNotificationService());

            await viewModel.RefreshCommand.ExecuteAsync(null);

            Assert.That(viewModel.Devices.Select(device => device.Serial), Is.EqualTo(new[] { "serial1" }));
            Assert.That(viewModel.SelectedDevice?.Serial, Is.EqualTo("serial1"));
            Assert.That(viewModel.StatusText, Is.EqualTo("1 device connected"));
        }

        [Test]
        public async Task It_reports_an_empty_list_when_no_devices_are_online()
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.Setup(s => s.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

            var viewModel = new MainWindowViewModel(deviceService.Object, Mock.Of<IDeviceOwnerService>(), new FakeNotificationService());

            await viewModel.RefreshCommand.ExecuteAsync(null);

            Assert.That(viewModel.IsListEmpty, Is.True);
            Assert.That(viewModel.CanSetDeviceOwner, Is.False);
            Assert.That(viewModel.StatusText, Is.EqualTo("No devices connected"));
        }
    }

    public class When_SetDeviceOwnerAsync_Is_Called : MainWindowViewModelTests
    {
        private static async Task<MainWindowViewModel> CreateWithSelectionAsync(IDeviceOwnerService deviceOwnerService, FakeNotificationService notifications)
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.Setup(s => s.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Online("serial1", "Pixel 7") });

            var viewModel = new MainWindowViewModel(deviceService.Object, deviceOwnerService, notifications);
            await viewModel.RefreshCommand.ExecuteAsync(null);
            return viewModel;
        }

        [Test]
        public async Task It_shows_a_success_notification_when_the_owner_is_set()
        {
            var deviceOwnerService = new Mock<IDeviceOwnerService>();
            deviceOwnerService.Setup(s => s.SetAsync("serial1", It.IsAny<CancellationToken>())).ReturnsAsync(DeviceOwnerResult.Succeeded());

            var notifications = new FakeNotificationService();
            var viewModel = await CreateWithSelectionAsync(deviceOwnerService.Object, notifications);

            await viewModel.SetDeviceOwnerCommand.ExecuteAsync(null);

            Assert.That(notifications.Shown.Single().Severity, Is.EqualTo(NotificationSeverity.Success));
            Assert.That(viewModel.StatusText, Is.EqualTo("Successfully set device owner"));
        }

        [Test]
        public async Task It_shows_an_error_notification_carrying_the_failure_message()
        {
            var deviceOwnerService = new Mock<IDeviceOwnerService>();
            deviceOwnerService.Setup(s => s.SetAsync("serial1", It.IsAny<CancellationToken>())).ReturnsAsync(DeviceOwnerResult.Failed("accounts already on device"));

            var notifications = new FakeNotificationService();
            var viewModel = await CreateWithSelectionAsync(deviceOwnerService.Object, notifications);

            await viewModel.SetDeviceOwnerCommand.ExecuteAsync(null);

            var shown = notifications.Shown.Single();
            Assert.That(shown.Severity, Is.EqualTo(NotificationSeverity.Error));
            Assert.That(shown.Message, Is.EqualTo("accounts already on device"));
            Assert.That(viewModel.StatusText, Is.EqualTo("Failed to set device owner"));
        }
    }
}
