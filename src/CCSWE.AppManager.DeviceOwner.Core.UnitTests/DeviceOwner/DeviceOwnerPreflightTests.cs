using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.Common;
using CCSWE.AppManager.DeviceOwner.Core.DeviceOwner;
using CCSWE.AppManager.DeviceOwner.Core.UnitTests.Fakes;
using Moq;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.DeviceOwner;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DeviceOwnerPreflightTests
{
    private const string CleanUsers = "Users:\n\tUserInfo{0:Owner:4c13} running\n";
    private const string InstalledPath = "package:/data/app/~~hash==/com.ccswe.appmanager.deviceowner-hash==/base.apk\n";
    private const string ZeroAccounts = "User UserInfo{0:Owner:4c13}:\n  Accounts: 0\n";

    private static DeviceOwnerPreflight Create(
        string listOwners = "",
        string listOwnersError = "",
        string path = InstalledPath,
        string users = CleanUsers,
        string accounts = ZeroAccounts)
    {
        var runner = new Mock<IProcessRunner>();

        void Setup(string token, ProcessResult result) =>
            runner.Setup(r => r.RunAsync(It.IsAny<string>(), It.Is<IReadOnlyList<string>>(a => a.Contains(token)), It.IsAny<CancellationToken>())).ReturnsAsync(result);

        Setup("list-owners", new ProcessResult(string.IsNullOrEmpty(listOwnersError) ? 0 : 1, listOwners, listOwnersError));
        Setup("path", new ProcessResult(0, path, string.Empty));
        Setup("users", new ProcessResult(0, users, string.Empty));
        Setup("account", new ProcessResult(0, accounts, string.Empty));

        var adbLocator = new Mock<IAdbLocator>();
        adbLocator.SetupGet(locator => locator.AdbPath).Returns("adb");

        return new DeviceOwnerPreflight(runner.Object, adbLocator.Object, new LoggerFake<DeviceOwnerPreflight>());
    }

    public class When_CheckAsync_Is_Called : DeviceOwnerPreflightTests
    {
        [Test]
        public async Task It_is_ready_on_a_clean_device()
        {
            var readiness = await Create().CheckAsync("serial1");

            Assert.That(readiness.IsReady, Is.True);
            Assert.That(readiness.AlreadyDeviceOwner, Is.False);
        }

        [Test]
        public async Task It_reports_already_owner_when_app_manager_owns_the_device()
        {
            var owners = "1 owner:\nUser  0: admin=com.ccswe.appmanager.deviceowner/com.ccswe.appmanager.receivers.DeviceAdminReceiver,DeviceOwner,Affiliated\n";

            var readiness = await Create(listOwners: owners).CheckAsync("serial1");

            Assert.That(readiness.AlreadyDeviceOwner, Is.True);
            Assert.That(readiness.Blockers, Is.Empty);
        }

        [Test]
        public async Task It_blocks_when_another_app_owns_the_device()
        {
            var owners = "1 owner:\nUser  0: admin=com.other.mdm/.Admin,DeviceOwner\n";

            var readiness = await Create(listOwners: owners).CheckAsync("serial1");

            Assert.That(readiness.AlreadyDeviceOwner, Is.False);
            Assert.That(readiness.Blockers.Single().Kind, Is.EqualTo(PreflightBlockerKind.DeviceOwnerByOther));
        }

        [Test]
        public async Task It_blocks_when_accounts_are_present()
        {
            var accounts = "User UserInfo{0:Owner:4c13}:\n  Accounts: 1\n    Account {name=testing@midworld.xyz, type=com.google}\n";

            var readiness = await Create(accounts: accounts).CheckAsync("serial1");

            Assert.That(readiness.Blockers.Select(b => b.Kind), Does.Contain(PreflightBlockerKind.AccountsPresent));
        }

        [Test]
        public async Task It_blocks_when_more_than_one_user_exists()
        {
            var users = "Users:\n\tUserInfo{0:Owner:4c13} running\n\tUserInfo{10:Work profile:1030} running\n";

            var readiness = await Create(users: users).CheckAsync("serial1");

            Assert.That(readiness.Blockers.Select(b => b.Kind), Does.Contain(PreflightBlockerKind.MultipleUsers));
        }

        [Test]
        public async Task It_blocks_when_app_manager_is_not_installed()
        {
            var readiness = await Create(path: string.Empty).CheckAsync("serial1");

            Assert.That(readiness.Blockers.Select(b => b.Kind), Does.Contain(PreflightBlockerKind.AppNotInstalled));
        }

        [Test]
        public async Task It_reports_not_connected_when_adb_says_the_device_is_offline()
        {
            var readiness = await Create(listOwnersError: "adb.exe: device offline").CheckAsync("serial1");

            Assert.That(readiness.Blockers.Single().Kind, Is.EqualTo(PreflightBlockerKind.NotConnected));
        }

        [Test]
        public async Task It_does_not_block_accounts_when_the_count_is_unknown_old_android()
        {
            var accounts = "User UserInfo{0:Owner:c13}:\n  Accounts History\n  RegisteredServicesCache: 8 services\n";

            var readiness = await Create(accounts: accounts).CheckAsync("serial1");

            Assert.That(readiness.Blockers.Select(b => b.Kind), Does.Not.Contain(PreflightBlockerKind.AccountsPresent));
            Assert.That(readiness.IsReady, Is.True);
        }
    }
}
