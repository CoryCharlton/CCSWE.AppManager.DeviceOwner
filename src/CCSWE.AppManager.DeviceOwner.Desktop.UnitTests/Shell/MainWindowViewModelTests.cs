using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Adb;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Desktop.Common;
using CCSWE.AppManager.DeviceOwner.Desktop.Common.Notifications;
using CCSWE.AppManager.DeviceOwner.Desktop.PlatformTools;
using CCSWE.AppManager.DeviceOwner.Desktop.Shell;
using CCSWE.AppManager.DeviceOwner.Desktop.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Desktop.UnitTests.Shell;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class MainWindowViewModelTests
{
    private static AdbDevice Online(string serial, string? model = null) => new(serial, "device", model, null, null, null);

    private static IAdbLocator AvailableAdb()
    {
        var locator = new Mock<IAdbLocator>();
        locator.SetupGet(l => l.IsAvailable).Returns(true);
        return locator.Object;
    }

    private static MainWindowViewModel Create(
        IDeviceService deviceService,
        IDeviceOwnerService? deviceOwnerService = null,
        INotificationService? notifications = null,
        IAdbLocator? adbLocator = null,
        IConfirmDialog? confirmDialog = null,
        IPlatformToolsInstallDialog? installDialog = null,
        FakeTimerFactory? timerFactory = null) =>
        new(
            deviceService,
            deviceOwnerService ?? Mock.Of<IDeviceOwnerService>(),
            notifications ?? new FakeNotificationService(),
            adbLocator ?? AvailableAdb(),
            confirmDialog ?? Mock.Of<IConfirmDialog>(),
            installDialog ?? Mock.Of<IPlatformToolsInstallDialog>(),
            timerFactory ?? new FakeTimerFactory());

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

            var viewModel = Create(deviceService.Object);

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

            var viewModel = Create(deviceService.Object);

            await viewModel.RefreshCommand.ExecuteAsync(null);

            Assert.That(viewModel.IsListEmpty, Is.True);
            Assert.That(viewModel.CanSetDeviceOwner, Is.False);
            Assert.That(viewModel.StatusText, Is.EqualTo("No devices connected"));
        }

        [Test]
        public async Task It_keeps_the_selected_device_across_a_refresh()
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.Setup(s => s.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Online("serial1", "Pixel 7"), Online("serial2", "Galaxy S24") });

            var viewModel = Create(deviceService.Object);
            await viewModel.RefreshCommand.ExecuteAsync(null);

            viewModel.SelectedDevice = viewModel.Devices.Single(row => row.Serial == "serial2");

            await viewModel.RefreshCommand.ExecuteAsync(null);

            Assert.That(viewModel.SelectedDevice?.Serial, Is.EqualTo("serial2"));
        }

        [Test]
        public async Task It_does_not_let_a_newly_connected_device_steal_the_selection()
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.SetupSequence(s => s.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Online("serial1", "Pixel 7") })
                .ReturnsAsync(new[] { Online("serial1", "Pixel 7"), Online("serial2", "Galaxy S24") });

            var viewModel = Create(deviceService.Object);
            await viewModel.RefreshCommand.ExecuteAsync(null);

            await viewModel.RefreshCommand.ExecuteAsync(null);

            Assert.That(viewModel.SelectedDevice?.Serial, Is.EqualTo("serial1"));
            Assert.That(viewModel.Devices, Has.Count.EqualTo(2));
        }

        [Test]
        public async Task It_reselects_when_the_selected_device_disconnects()
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.SetupSequence(s => s.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Online("serial1", "Pixel 7"), Online("serial2", "Galaxy S24") })
                .ReturnsAsync(new[] { Online("serial1", "Pixel 7") });

            var viewModel = Create(deviceService.Object);
            await viewModel.RefreshCommand.ExecuteAsync(null);
            viewModel.SelectedDevice = viewModel.Devices.Single(row => row.Serial == "serial2");

            await viewModel.RefreshCommand.ExecuteAsync(null);

            Assert.That(viewModel.SelectedDevice?.Serial, Is.EqualTo("serial1"));
        }
    }

    public class When_The_Refresh_Timer_Ticks : MainWindowViewModelTests
    {
        [Test]
        public async Task It_relists_devices_silently()
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.SetupSequence(s => s.ListAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { Online("serial1", "Pixel 7") })
                .ReturnsAsync(new[] { Online("serial1", "Pixel 7"), Online("serial2", "Galaxy S24") });

            var timerFactory = new FakeTimerFactory();
            var viewModel = Create(deviceService.Object, timerFactory: timerFactory);
            await viewModel.RefreshCommand.ExecuteAsync(null);

            timerFactory.Timer.FireTick();

            Assert.That(viewModel.Devices.Select(device => device.Serial), Is.EquivalentTo(new[] { "serial1", "serial2" }));
            Assert.That(timerFactory.Timer.IsRunning, Is.True);
        }

        [Test]
        public void It_does_not_list_or_prompt_while_adb_is_unavailable()
        {
            var deviceService = new Mock<IDeviceService>();
            var installDialog = new Mock<IPlatformToolsInstallDialog>();

            var locator = new Mock<IAdbLocator>();
            locator.SetupGet(l => l.IsAvailable).Returns(false);

            var timerFactory = new FakeTimerFactory();
            _ = Create(deviceService.Object, adbLocator: locator.Object, installDialog: installDialog.Object, timerFactory: timerFactory);

            timerFactory.Timer.FireTick();

            deviceService.Verify(s => s.ListAsync(It.IsAny<CancellationToken>()), Times.Never);
            installDialog.Verify(d => d.ShowAsync(), Times.Never);
        }
    }

    public class When_Adb_Is_Not_Found : MainWindowViewModelTests
    {
        private static IAdbLocator MissingAdb()
        {
            var locator = new Mock<IAdbLocator>();
            locator.SetupGet(l => l.IsAvailable).Returns(false);
            return locator.Object;
        }

        [Test]
        public async Task It_scans_after_the_install_dialog_reports_success()
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.Setup(s => s.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Online("serial1", "Pixel 7") });

            var confirmDialog = new Mock<IConfirmDialog>();
            confirmDialog.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var installDialog = new Mock<IPlatformToolsInstallDialog>();
            installDialog.Setup(d => d.ShowAsync()).ReturnsAsync(true);

            var viewModel = Create(deviceService.Object, adbLocator: MissingAdb(), confirmDialog: confirmDialog.Object, installDialog: installDialog.Object);

            await viewModel.RefreshCommand.ExecuteAsync(null);

            Assert.That(viewModel.Devices.Select(device => device.Serial), Is.EqualTo(new[] { "serial1" }));
            installDialog.Verify(d => d.ShowAsync(), Times.Once);
        }

        [Test]
        public async Task It_does_not_scan_when_the_user_declines_the_offer()
        {
            var deviceService = new Mock<IDeviceService>();

            var confirmDialog = new Mock<IConfirmDialog>();
            confirmDialog.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var viewModel = Create(deviceService.Object, adbLocator: MissingAdb(), confirmDialog: confirmDialog.Object);

            await viewModel.RefreshCommand.ExecuteAsync(null);

            deviceService.Verify(s => s.ListAsync(It.IsAny<CancellationToken>()), Times.Never);
            Assert.That(viewModel.StatusText, Is.EqualTo("adb not found"));
        }

        [Test]
        public async Task It_does_not_scan_when_the_install_is_cancelled()
        {
            var deviceService = new Mock<IDeviceService>();

            var confirmDialog = new Mock<IConfirmDialog>();
            confirmDialog.Setup(d => d.ConfirmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);

            var installDialog = new Mock<IPlatformToolsInstallDialog>();
            installDialog.Setup(d => d.ShowAsync()).ReturnsAsync(false);

            var viewModel = Create(deviceService.Object, adbLocator: MissingAdb(), confirmDialog: confirmDialog.Object, installDialog: installDialog.Object);

            await viewModel.RefreshCommand.ExecuteAsync(null);

            deviceService.Verify(s => s.ListAsync(It.IsAny<CancellationToken>()), Times.Never);
            Assert.That(viewModel.StatusText, Is.EqualTo("adb not found"));
        }
    }

    public class When_SetDeviceOwnerAsync_Is_Called : MainWindowViewModelTests
    {
        private static async Task<MainWindowViewModel> CreateWithSelectionAsync(IDeviceOwnerService deviceOwnerService, FakeNotificationService notifications)
        {
            var deviceService = new Mock<IDeviceService>();
            deviceService.Setup(s => s.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Online("serial1", "Pixel 7") });

            var viewModel = Create(deviceService.Object, deviceOwnerService, notifications);
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
