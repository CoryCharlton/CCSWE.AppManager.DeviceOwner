using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Settings;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.Settings;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SettingsServiceTests
{
    public class When_A_Setting_Changes : SettingsServiceTests
    {
        [Test]
        public void It_persists_a_changed_theme()
        {
            var store = new Mock<ISettingsStore>();
            store.Setup(s => s.Load()).Returns(new SettingsModel { Theme = AppTheme.Dark });

            var service = new SettingsService(store.Object);
            service.Theme = AppTheme.Light;

            Assert.That(service.Theme, Is.EqualTo(AppTheme.Light));
            store.Verify(s => s.Save(It.Is<SettingsModel>(model => model.Theme == AppTheme.Light)), Times.Once);
        }

        [Test]
        public void It_does_not_persist_when_the_value_is_unchanged()
        {
            var store = new Mock<ISettingsStore>();
            store.Setup(s => s.Load()).Returns(new SettingsModel { Theme = AppTheme.Dark });

            var service = new SettingsService(store.Object);
            service.Theme = AppTheme.Dark;

            store.Verify(s => s.Save(It.IsAny<SettingsModel>()), Times.Never);
        }

        [Test]
        public void It_normalizes_a_blank_adb_path_override_to_null()
        {
            var store = new Mock<ISettingsStore>();
            store.Setup(s => s.Load()).Returns(new SettingsModel { AdbPath = "/sdk/adb" });

            var service = new SettingsService(store.Object);
            service.AdbPath = "   ";

            Assert.That(service.AdbPath, Is.Null);
            store.Verify(s => s.Save(It.Is<SettingsModel>(model => model.AdbPath == null)), Times.Once);
        }
    }
}
