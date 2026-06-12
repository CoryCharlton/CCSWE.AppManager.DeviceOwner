using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Adb;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.Adb;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeviceDetailsParserTests
{
    public class When_Build_Is_Called
    {
        [Test]
        public void It_names_a_physical_device_by_its_model()
        {
            // Real Pixel 8 Pro getprop output.
            var output = "ro.kernel.qemu=\nro.boot.qemu.avd_name=\nro.build.characteristics=nosdcard\n"
                + "ro.product.model=Pixel 8 Pro\nro.product.marketing.name=\nro.product.vendor.marketname=\n";

            var details = DeviceDetailsParser.Build("3B100DLJG001J6", output);

            Assert.That(details.Name, Is.EqualTo("Pixel 8 Pro"));
            Assert.That(details.Form, Is.EqualTo(DeviceForm.Phone));
            Assert.That(details.IsEmulator, Is.False);
        }

        [Test]
        public void It_names_an_emulator_after_its_avd()
        {
            // Real Android 17 emulator getprop output.
            var output = "ro.kernel.qemu=1\nro.boot.qemu.avd_name=Pixel_9\nro.build.characteristics=emulator\n"
                + "ro.product.model=sdk_gphone16k_x86_64\n";

            var details = DeviceDetailsParser.Build("emulator-5554", output);

            Assert.That(details.Name, Is.EqualTo("Pixel 9"));
            Assert.That(details.IsEmulator, Is.True);
            Assert.That(details.Form, Is.EqualTo(DeviceForm.Phone));
        }

        [Test]
        public void It_treats_an_emulator_serial_as_an_emulator()
        {
            var details = DeviceDetailsParser.Build("emulator-5556", "ro.boot.qemu.avd_name=Tablet_API_35\n");

            Assert.That(details.IsEmulator, Is.True);
            Assert.That(details.Name, Is.EqualTo("Tablet API 35"));
        }

        [Test]
        public void It_reads_the_form_factor_from_characteristics()
        {
            var details = DeviceDetailsParser.Build("R5CW1234", "ro.build.characteristics=tablet\nro.product.model=SM-X710\n");

            Assert.That(details.Form, Is.EqualTo(DeviceForm.Tablet));
        }

        [Test]
        public void It_prefers_a_marketing_name_over_the_model()
        {
            var output = "ro.product.model=SM-S911B\nro.product.vendor.marketname=Galaxy S23\n";

            Assert.That(DeviceDetailsParser.Build("R5CW1234", output).Name, Is.EqualTo("Galaxy S23"));
        }

        [Test]
        public void It_returns_a_null_name_when_none_can_be_built()
        {
            var details = DeviceDetailsParser.Build("serial1", string.Empty);

            Assert.That(details.Name, Is.Null);
            Assert.That(details.Form, Is.EqualTo(DeviceForm.Phone));
            Assert.That(details.IsEmulator, Is.False);
        }
    }
}
