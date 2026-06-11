using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.PlatformTools;
using CCSWE.AppManager.DeviceOwner.Desktop.PlatformTools;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Desktop.UnitTests.PlatformTools;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DownloadProgressDialogViewModelTests
{
    public class When_ApplyProgress_Is_Called
    {
        [Test]
        public void It_maps_a_determinate_sample_onto_the_progress_bar()
        {
            var viewModel = new DownloadProgressDialogViewModel(Mock.Of<IPlatformToolsInstaller>());

            viewModel.ApplyProgress(new DownloadProgress(512, 1024, 256, TimeSpan.FromSeconds(2)));

            Assert.That(viewModel.IsIndeterminate, Is.False);
            Assert.That(viewModel.PercentComplete, Is.EqualTo(50).Within(0.001));
            Assert.That(viewModel.StatusLine, Is.EqualTo("512 B of 1.0 KB"));
            Assert.That(viewModel.SpeedAndEtaLine, Is.EqualTo("256 B/s · 0:02 remaining"));
        }

        [Test]
        public void It_stays_indeterminate_when_the_total_is_unknown()
        {
            var viewModel = new DownloadProgressDialogViewModel(Mock.Of<IPlatformToolsInstaller>());

            viewModel.ApplyProgress(new DownloadProgress(512, null, 0, null));

            Assert.That(viewModel.IsIndeterminate, Is.True);
            Assert.That(viewModel.StatusLine, Is.EqualTo("512 B downloaded"));
            Assert.That(viewModel.SpeedAndEtaLine, Is.Empty);
        }
    }

    public class When_RunAsync_Completes
    {
        [Test]
        public async Task It_closes_with_success_when_the_install_succeeds()
        {
            var installer = new Mock<IPlatformToolsInstaller>();
            installer.Setup(i => i.InstallAsync(It.IsAny<IProgress<DownloadProgress>>(), It.IsAny<CancellationToken>())).ReturnsAsync("/sdk/platform-tools/adb");

            var viewModel = new DownloadProgressDialogViewModel(installer.Object);
            bool? result = null;
            viewModel.CloseRequested += value => result = value;

            await viewModel.RunAsync();

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task It_surfaces_the_error_and_offers_to_close_when_the_install_fails()
        {
            var installer = new Mock<IPlatformToolsInstaller>();
            installer.Setup(i => i.InstallAsync(It.IsAny<IProgress<DownloadProgress>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("connection reset"));

            var viewModel = new DownloadProgressDialogViewModel(installer.Object);

            await viewModel.RunAsync();

            Assert.That(viewModel.IsRunning, Is.False);
            Assert.That(viewModel.CancelLabel, Is.EqualTo("Close"));
            Assert.That(viewModel.StatusLine, Does.Contain("download server"));
        }
    }

    public class When_Cancel_Is_Invoked
    {
        [Test]
        public async Task It_cancels_the_install_and_closes_with_failure()
        {
            var started = new TaskCompletionSource();
            var installer = new Mock<IPlatformToolsInstaller>();
            installer
                .Setup(i => i.InstallAsync(It.IsAny<IProgress<DownloadProgress>>(), It.IsAny<CancellationToken>()))
                .Returns(async (IProgress<DownloadProgress> _, CancellationToken cancellationToken) =>
                {
                    started.SetResult();
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                    return string.Empty;
                });

            var viewModel = new DownloadProgressDialogViewModel(installer.Object);
            bool? result = null;
            viewModel.CloseRequested += value => result = value;

            var run = viewModel.RunAsync();
            await started.Task;
            viewModel.CancelCommand.Execute(null);
            await run;

            Assert.That(result, Is.False);
        }
    }
}
