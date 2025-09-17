using NUnit.Framework;
using SoopExtension.Editor;

namespace SoopExtension.Tests.Editor
{
    public class SoopSettingsTests
    {
        [Test]
        public void SoopSettings_GetOrCreateSettings_ReturnsValidInstance()
        {
            var settings = SoopSettings.GetOrCreateSettings();

            Assert.IsNotNull(settings);
            Assert.IsNotNull(settings.baseUrls);
            Assert.IsFalse(string.IsNullOrEmpty(settings.userAgent));
        }

        [Test]
        public void SoopSettings_DefaultValues_AreCorrect()
        {
            var settings = SoopSettings.GetOrCreateSettings();

            Assert.AreEqual("https://live.sooplive.co.kr", settings.baseUrls.soopLiveBaseUrl);
            Assert.AreEqual("https://chapi.sooplive.co.kr", settings.baseUrls.soopChannelBaseUrl);
            Assert.AreEqual("https://login.sooplive.co.kr", settings.baseUrls.soopAuthBaseUrl);
        }

        [Test]
        public void SoopSettingsProvider_IsAvailable()
        {
            bool isAvailable = SoopSettingsProvider.IsSettingsAvailable();
            Assert.IsTrue(isAvailable);
        }
    }
}