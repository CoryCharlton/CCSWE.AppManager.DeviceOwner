using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.PlatformTools;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DownloadFormatTests
{
    public class When_Bytes_Is_Called
    {
        [Test]
        public void It_renders_whole_bytes_without_a_decimal()
        {
            Assert.That(DownloadFormat.Bytes(512), Is.EqualTo("512 B"));
        }

        [Test]
        public void It_renders_larger_units_with_one_decimal()
        {
            Assert.That(DownloadFormat.Bytes(1536), Is.EqualTo("1.5 KB"));
        }
    }

    public class When_SpeedAndEta_Is_Called
    {
        [Test]
        public void It_appends_the_remaining_time_when_an_eta_is_known()
        {
            Assert.That(DownloadFormat.SpeedAndEta(1024, TimeSpan.FromSeconds(9)), Is.EqualTo("1.0 KB/s · 0:09 remaining"));
        }

        [Test]
        public void It_renders_speed_only_when_the_eta_is_unknown()
        {
            Assert.That(DownloadFormat.SpeedAndEta(2048, null), Is.EqualTo("2.0 KB/s"));
        }
    }
}
