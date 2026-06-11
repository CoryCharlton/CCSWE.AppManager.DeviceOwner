using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.Settings;
using CCSWE.AppManager.DeviceOwner.Core.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.Common;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AdbLocatorTests
{
    private static AdbLocator Create(string? overridePath, string? onPath)
    {
        var settings = new Mock<ISettingsService>();
        settings.SetupGet(s => s.AdbPath).Returns(overridePath);

        var finder = new Mock<IExecutableFinder>();
        finder.Setup(f => f.FindOnPath("adb")).Returns(onPath);

        return new AdbLocator(settings.Object, finder.Object, new LoggerFake<AdbLocator>());
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
        public void It_is_false_when_nothing_resolves()
        {
            Assert.That(Create(null, null).IsAvailable, Is.False);
        }
    }
}
