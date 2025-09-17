using System;

namespace SoopExtension
{
    [Serializable]
    public class SoopAPIBaseUrls
    {
        public string soopLiveBaseUrl = "https://live.sooplive.co.kr";
        public string soopChannelBaseUrl = "https://chapi.sooplive.co.kr";
        public string soopAuthBaseUrl = "https://login.sooplive.co.kr";
    }

    [Serializable]
    public class SoopClientOptions
    {
        public SoopAPIBaseUrls baseUrls;
        public string userAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/37.0.2049.0 Safari/537.36";

        public SoopClientOptions()
        {
            baseUrls = new SoopAPIBaseUrls();
        }
    }

    [Serializable]
    public class SoopLoginOptions
    {
        public string userId;
        public string password;
    }

    [Serializable]
    public class SoopChatOptions
    {
        public string streamerId;
        public SoopLoginOptions login;
        public SoopAPIBaseUrls baseUrls;
    }
}
