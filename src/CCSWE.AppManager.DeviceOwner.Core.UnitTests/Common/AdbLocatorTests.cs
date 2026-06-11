using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.Settings;
using CCSWE.AppManager.DeviceOwner.Core.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.Common;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AdbLocatorTests
{
    private static AdbLocator Create(string? overridePath, string? onPath, IEnvironment? environment = null)
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.AdbPath).Returns(overridePath);

        var finder = new Mock<IExecutableFinder>();
        finder.Setup(f => f.FindOnPath("adb")).Returns(onPath);

        return new AdbLocator(settings.Object, finder.Object, environment ?? EmptyEnvironment(), new LoggerFake<AdbLocator>());
    }

    // No environment variables set and the default SDK root pointed at a folder with no adb, so resolution falls
    // through to the override and PATH (both controlled by the test).
    private static IEnvironment EmptyEnvironment()
    {
        var environment = new Mock<IEnvironment>();
        environment.Setup(e => e.GetEnvironmentVariable(It.IsAny<string>())).Returns((string?)null);
        environment.Setup(e => e.GetFolderPath(It.IsAny<Environment.SpecialFolder>())).Returns(Path.Combine(Path.GetTempPath(), $"adb-locator-tests-{Guid.NewGuid():N}"));
        return environment.Object;
    }

    public class When_IsAvailable_Is_Read : AdbLocatorTests
    {
        [Test]
        public void It_is_true_when_the_override_points_at_a_real_file()
        {
            var file = Path.GetTempFileName();

            try
            {
                Assert.That(Create(file, null).IsAvailable, Is.True);
            }
            finally
            {
                File.Delete(file);
            }
        }

        [Test]
        public void It_is_true_when_an_environment_variable_points_at_platform_tools()
        {
            var root = Directory.CreateTempSubdirectory("adb-locator-tests");
            var platformTools = Directory.CreateDirectory(Path.Combine(root.FullName, "platform-tools"));
            var adb = Path.Combine(platformTools.FullName, OperatingSystem.IsWindows() ? "adb.exe" : "adb");
            File.WriteAllText(adb, string.Empty);

            try
            {
                var environment = new Mock<IEnvironment>();
                environment.Setup(e => e.GetEnvironmentVariable("ANDROID_HOME")).Returns(root.FullName);
                environment.Setup(e => e.GetFolderPath(It.IsAny<Environment.SpecialFolder>())).Returns(Path.Combine(Path.GetTempPath(), $"adb-locator-tests-{Guid.NewGuid():N}"));

                Assert.That(Create(null, null, environment.Object).IsAvailable, Is.True);
            }
            finally
            {
                root.Delete(recursive: true);
            }
        }

        [Test]
        public void It_is_false_when_nothing_resolves()
        {
            Assert.That(Create(null, null).IsAvailable, Is.False);
        }
    }
}
