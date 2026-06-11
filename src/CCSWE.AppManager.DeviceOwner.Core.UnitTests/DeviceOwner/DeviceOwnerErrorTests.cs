using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.DeviceOwner;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeviceOwnerErrorTests
{
    public class When_Describe_Is_Called
    {
        [Test]
        public void It_maps_the_accounts_error()
        {
            var output = "java.lang.IllegalStateException: Not allowed to set the device owner because there are already some accounts on the device.";

            Assert.That(DeviceOwnerError.Describe(output), Is.EqualTo(DeviceOwnerMessages.AccountsPresent));
        }

        [Test]
        public void It_maps_the_verbatim_android_17_accounts_failure()
        {
            // Captured live from a Pixel 9 emulator (Android 17): full stderr including the wrapper and stack trace.
            var output = "Exception occurred while executing 'set-device-owner':\n"
                + "java.lang.IllegalStateException: Not allowed to set the device owner because there are already some accounts on the device.\n"
                + "\tat com.android.server.devicepolicy.DevicePolicyManagerService.enforceCanSetDeviceOwnerLocked(DevicePolicyManagerService.java:10610)\n";

            Assert.That(DeviceOwnerError.Describe(output), Is.EqualTo(DeviceOwnerMessages.AccountsPresent));
        }

        [Test]
        public void It_maps_the_several_users_error()
        {
            var output = "java.lang.IllegalStateException: Not allowed to set the device owner because there are already several users on the device.";

            Assert.That(DeviceOwnerError.Describe(output), Is.EqualTo(DeviceOwnerMessages.Users));
        }

        [Test]
        public void It_maps_the_already_provisioned_error()
        {
            Assert.That(DeviceOwnerError.Describe("java.lang.IllegalStateException: Trying to set device owner but device is already provisioned."), Is.EqualTo(DeviceOwnerMessages.AlreadyProvisioned));
        }

        [Test]
        public void It_maps_the_already_set_up_variant()
        {
            Assert.That(DeviceOwnerError.Describe("java.lang.IllegalStateException: Cannot set the device owner if the device is already set-up."), Is.EqualTo(DeviceOwnerMessages.AlreadyProvisioned));
        }

        [Test]
        public void It_maps_the_existing_device_owner_error()
        {
            Assert.That(DeviceOwnerError.Describe("java.lang.IllegalStateException: Trying to set device owner but device owner is already set."), Is.EqualTo(DeviceOwnerMessages.DeviceOwnerByOther));
        }

        [Test]
        public void It_maps_an_unknown_admin_to_not_installed()
        {
            Assert.That(DeviceOwnerError.Describe("Error: Unknown admin: ComponentInfo{com.ccswe.appmanager.deviceowner/...}"), Is.EqualTo(DeviceOwnerMessages.AppNotInstalled));
        }

        [Test]
        public void It_maps_a_transport_error_to_not_connected()
        {
            Assert.That(DeviceOwnerError.Describe("adb.exe: device unauthorized."), Is.EqualTo(DeviceOwnerMessages.NotConnected));
        }

        [Test]
        public void It_falls_back_to_the_raw_text_for_an_unrecognized_failure()
        {
            var output = "java.lang.RuntimeException: Can't set package as device owner.";

            Assert.That(DeviceOwnerError.Describe(output), Is.EqualTo(output));
        }

        [Test]
        public void It_describes_an_empty_failure()
        {
            Assert.That(DeviceOwnerError.Describe(string.Empty), Does.Contain("without any details"));
        }
    }
}
