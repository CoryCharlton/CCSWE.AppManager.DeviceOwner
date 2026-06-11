using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using CCSWE.AppManager.DeviceOwner.Core.PlatformTools;
using CCSWE.AppManager.DeviceOwner.Core.Settings;
using CCSWE.AppManager.DeviceOwner.Core.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.PlatformTools;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class PlatformToolsInstallerTests
{
    public class When_DownloadUrlFor_Is_Called
    {
        [Test]
        public void It_selects_the_windows_archive()
        {
            Assert.That(PlatformToolsInstaller.DownloadUrlFor(true, false, false), Is.EqualTo("https://dl.google.com/android/repository/platform-tools-latest-windows.zip"));
        }

        [Test]
        public void It_selects_the_macos_archive()
        {
            Assert.That(PlatformToolsInstaller.DownloadUrlFor(false, true, false), Is.EqualTo("https://dl.google.com/android/repository/platform-tools-latest-darwin.zip"));
        }

        [Test]
        public void It_selects_the_linux_archive()
        {
            Assert.That(PlatformToolsInstaller.DownloadUrlFor(false, false, true), Is.EqualTo("https://dl.google.com/android/repository/platform-tools-latest-linux.zip"));
        }

        [Test]
        public void It_throws_for_an_unsupported_platform()
        {
            Assert.Throws<PlatformNotSupportedException>(() => PlatformToolsInstaller.DownloadUrlFor(false, false, false));
        }
    }

    public class When_InstallRoot_Is_Called
    {
        [Test]
        public void It_lives_under_local_application_data()
        {
            var expected = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CCSWE.AppManager.DeviceOwner");

            Assert.That(PlatformToolsInstaller.InstallRoot(), Is.EqualTo(expected));
        }
    }

    public class When_ExtractAndRegisterAsync_Is_Called
    {
        [Test]
        public async Task It_extracts_adb_and_records_it_as_the_override()
        {
            var installRoot = Path.Combine(Path.GetTempPath(), $"platform-tools-test-{Guid.NewGuid():N}");

            try
            {
                var settings = new Mock<ISettingsService>();
                settings.SetupProperty(s => s.AdbPath);

                var installer = new PlatformToolsInstaller(settings.Object, Mock.Of<IHttpClientFactory>(), new LoggerFake<PlatformToolsInstaller>(), installRoot);

                await using var zip = CreatePlatformToolsZip();
                var adbPath = await installer.ExtractAndRegisterAsync(zip, CancellationToken.None);

                Assert.That(File.Exists(adbPath), Is.True);
                Assert.That(adbPath, Does.StartWith(Path.Combine(installRoot, "platform-tools")));
                Assert.That(settings.Object.AdbPath, Is.EqualTo(adbPath));
            }
            finally
            {
                if (Directory.Exists(installRoot))
                {
                    Directory.Delete(installRoot, recursive: true);
                }
            }
        }

        [Test]
        public void It_throws_when_the_archive_has_no_platform_tools_folder()
        {
            var installRoot = Path.Combine(Path.GetTempPath(), $"platform-tools-test-{Guid.NewGuid():N}");

            try
            {
                var installer = new PlatformToolsInstaller(Mock.Of<ISettingsService>(), Mock.Of<IHttpClientFactory>(), new LoggerFake<PlatformToolsInstaller>(), installRoot);

                using var zip = CreateZip("readme.txt");

                Assert.ThrowsAsync<InvalidOperationException>(() => installer.ExtractAndRegisterAsync(zip, CancellationToken.None));
            }
            finally
            {
                if (Directory.Exists(installRoot))
                {
                    Directory.Delete(installRoot, recursive: true);
                }
            }
        }

        private static MemoryStream CreatePlatformToolsZip() =>
            CreateZip("platform-tools/adb", "platform-tools/adb.exe", "platform-tools/source.properties");

        private static MemoryStream CreateZip(params string[] entries)
        {
            var buffer = new MemoryStream();

            using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var name in entries)
                {
                    using var stream = archive.CreateEntry(name).Open();
                    stream.Write("binary"u8);
                }
            }

            buffer.Position = 0;
            return buffer;
        }
    }
}
