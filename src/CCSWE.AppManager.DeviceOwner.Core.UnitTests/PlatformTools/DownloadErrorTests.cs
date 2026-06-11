using System.Diagnostics.CodeAnalysis;
using System.Net;
using CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.PlatformTools;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DownloadErrorTests
{
    public class When_Describe_Is_Called
    {
        [Test]
        public void It_reports_the_status_code_for_an_http_error_response()
        {
            var message = DownloadError.Describe(new HttpRequestException("nope", null, HttpStatusCode.NotFound));

            Assert.That(message, Does.Contain("404"));
        }

        [Test]
        public void It_points_at_connectivity_for_a_status_less_http_failure()
        {
            var message = DownloadError.Describe(new HttpRequestException("no such host"));

            Assert.That(message, Does.Contain("internet connection"));
        }

        [Test]
        public void It_describes_a_timeout()
        {
            Assert.That(DownloadError.Describe(new TaskCanceledException()), Does.Contain("timed out"));
        }

        [Test]
        public void It_describes_a_disk_failure()
        {
            Assert.That(DownloadError.Describe(new UnauthorizedAccessException()), Does.Contain("disk space"));
        }

        [Test]
        public void It_falls_back_to_the_message_of_an_installer_thrown_exception()
        {
            var message = DownloadError.Describe(new InvalidOperationException("adb was not found after extraction."));

            Assert.That(message, Is.EqualTo("adb was not found after extraction."));
        }
    }
}
