using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Adb;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.Adb;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AdbOutputParserTests
{
    public class When_ParseDeviceList_Is_Called
    {
        [Test]
        public void It_captures_the_descriptive_columns_of_online_devices()
        {
            var output = "List of devices attached\n"
                + "emulator-5554          device product:sdk_gphone64 model:sdk_gphone64_x86_64 device:emu64xa transport_id:1\n";

            var devices = AdbOutputParser.ParseDeviceList(output);

            var device = devices.Single();
            Assert.That(device.Serial, Is.EqualTo("emulator-5554"));
            Assert.That(device.State, Is.EqualTo("device"));
            Assert.That(device.IsOnline, Is.True);
            Assert.That(device.Model, Is.EqualTo("sdk_gphone64_x86_64"));
            Assert.That(device.Product, Is.EqualTo("sdk_gphone64"));
            Assert.That(device.Device, Is.EqualTo("emu64xa"));
            Assert.That(device.TransportId, Is.EqualTo("1"));
        }

        [Test]
        public void It_preserves_offline_and_unauthorized_states()
        {
            var output = "List of devices attached\n"
                + "emulator-5554\toffline\n"
                + "abc123\tunauthorized\n"
                + "xyz789\tdevice model:Pixel_7\n";

            var devices = AdbOutputParser.ParseDeviceList(output);

            Assert.That(devices.Select(device => device.State), Is.EqualTo(new[] { "offline", "unauthorized", "device" }));
            Assert.That(devices.Count(device => device.IsOnline), Is.EqualTo(1));
        }

        [Test]
        public void It_returns_empty_when_no_devices_attached()
        {
            var output = "List of devices attached\n\n";

            var devices = AdbOutputParser.ParseDeviceList(output);

            Assert.That(devices, Is.Empty);
        }
    }
}
