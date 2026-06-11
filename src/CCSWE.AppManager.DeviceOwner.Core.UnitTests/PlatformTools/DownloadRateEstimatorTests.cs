using System.Diagnostics.CodeAnalysis;
using CCSWE.AppManager.DeviceOwner.Core.PlatformTools;

namespace CCSWE.AppManager.DeviceOwner.Core.UnitTests.PlatformTools;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DownloadRateEstimatorTests
{
    public class When_Update_Is_Called
    {
        [Test]
        public void It_seeds_the_rate_from_the_first_sample()
        {
            var estimator = new DownloadRateEstimator();

            Assert.That(estimator.Update(1000, 1.0), Is.EqualTo(1000).Within(0.001));
        }

        [Test]
        public void It_smooths_subsequent_samples_toward_the_new_rate()
        {
            var estimator = new DownloadRateEstimator();
            estimator.Update(1000, 1.0);

            var smoothed = estimator.Update(2000, 1.0);

            Assert.That(smoothed, Is.EqualTo(0.3 * 2000 + 0.7 * 1000).Within(0.001));
        }

        [Test]
        public void It_ignores_a_sample_with_no_elapsed_time()
        {
            var estimator = new DownloadRateEstimator();
            estimator.Update(1000, 1.0);

            Assert.That(estimator.Update(5000, 0), Is.EqualTo(1000).Within(0.001));
        }
    }

    public class When_Eta_Is_Called
    {
        [Test]
        public void It_estimates_the_remaining_time_from_the_smoothed_rate()
        {
            var estimator = new DownloadRateEstimator();
            estimator.Update(1000, 1.0);

            Assert.That(estimator.Eta(0, 2000), Is.EqualTo(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void It_returns_null_when_the_total_is_unknown()
        {
            var estimator = new DownloadRateEstimator();
            estimator.Update(1000, 1.0);

            Assert.That(estimator.Eta(0, null), Is.Null);
        }

        [Test]
        public void It_returns_null_before_a_rate_is_known()
        {
            Assert.That(new DownloadRateEstimator().Eta(0, 2000), Is.Null);
        }
    }
}
