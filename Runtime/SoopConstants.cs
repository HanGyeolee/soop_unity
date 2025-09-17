namespace SoopExtension
{
    public static class SoopConstants
    {
        public static readonly SoopAPIBaseUrls DEFAULT_BASE_URLS = new SoopAPIBaseUrls
        {
            soopLiveBaseUrl = "https://live.sooplive.co.kr",
            soopChannelBaseUrl = "https://chapi.sooplive.co.kr",
            soopAuthBaseUrl = "https://login.sooplive.co.kr"
        };

        public const string DEFAULT_USER_AGENT = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36";
    }
}
