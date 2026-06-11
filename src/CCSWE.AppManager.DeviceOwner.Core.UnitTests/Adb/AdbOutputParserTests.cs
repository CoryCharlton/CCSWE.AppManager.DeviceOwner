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

    public class When_ParseOwners_Is_Called
    {
        [Test]
        public void It_returns_empty_for_no_owners()
        {
            Assert.That(AdbOutputParser.ParseOwners(string.Empty), Is.Empty);
        }

        [Test]
        public void It_parses_app_manager_as_the_device_owner()
        {
            var output = "1 owner:\n"
                + "User  0: admin=com.ccswe.appmanager.deviceowner/com.ccswe.appmanager.receivers.DeviceAdminReceiver,DeviceOwner,Affiliated\n";

            var owner = AdbOutputParser.ParseOwners(output).Single();

            Assert.That(owner.UserId, Is.EqualTo(0));
            Assert.That(owner.Package, Is.EqualTo("com.ccswe.appmanager.deviceowner"));
            Assert.That(owner.IsDeviceOwner, Is.True);
            Assert.That(owner.IsProfileOwner, Is.False);
        }

        [Test]
        public void It_parses_another_app_as_the_device_owner()
        {
            var output = "1 owner:\nUser  0: admin=com.other.mdm/.AdminReceiver,DeviceOwner\n";

            var owner = AdbOutputParser.ParseOwners(output).Single();

            Assert.That(owner.Package, Is.EqualTo("com.other.mdm"));
            Assert.That(owner.IsDeviceOwner, Is.True);
        }

        [Test]
        public void It_distinguishes_a_profile_owner()
        {
            var output = "1 owner:\nUser 10: admin=com.work/.Admin,ProfileOwner\n";

            var owner = AdbOutputParser.ParseOwners(output).Single();

            Assert.That(owner.UserId, Is.EqualTo(10));
            Assert.That(owner.IsDeviceOwner, Is.False);
            Assert.That(owner.IsProfileOwner, Is.True);
        }
    }

    public class When_ParseUserCount_Is_Called
    {
        [Test]
        public void It_counts_a_single_user()
        {
            var output = "Users:\n\tUserInfo{0:Owner:4c13} running\n";

            Assert.That(AdbOutputParser.ParseUserCount(output), Is.EqualTo(1));
        }

        [Test]
        public void It_counts_multiple_users()
        {
            var output = "Users:\n\tUserInfo{0:Owner:4c13} running\n\tUserInfo{10:Work profile:1030} running\n";

            Assert.That(AdbOutputParser.ParseUserCount(output), Is.EqualTo(2));
        }
    }

    public class When_ParseAccountCount_Is_Called
    {
        [Test]
        public void It_reads_zero_accounts()
        {
            var output = "User UserInfo{0:Owner:4c13}:\n  Accounts: 0\n";

            Assert.That(AdbOutputParser.ParseAccountCount(output), Is.EqualTo(0));
        }

        [Test]
        public void It_reads_a_present_account_without_counting_session_references()
        {
            var output = "User UserInfo{0:Owner:4c13}:\n"
                + "  Accounts: 1\n"
                + "    Account {name=testing@midworld.xyz, type=com.google}\n"
                + "  Active Sessions: 2\n"
                + "    Session: Account {name=testing@midworld.xyz, type=com.google}\n";

            Assert.That(AdbOutputParser.ParseAccountCount(output), Is.EqualTo(1));
        }

        [Test]
        public void It_returns_null_when_no_accounts_line_is_present()
        {
            var output = "User UserInfo{0:Owner:c13}:\n  Accounts History\n  RegisteredServicesCache: 8 services\n";

            Assert.That(AdbOutputParser.ParseAccountCount(output), Is.Null);
        }
    }
}
