using UnityEngine;
using SoopExtension;

namespace SoopExtension.Samples
{
    public class SoopAPIExample : MonoBehaviour
    {
        [Header("API Test Settings")]
        public string streamerId = "your_streamer_id";
        public string userId = "your_user_id";
        public string password = "your_password";

        [Header("UI References")]
        public UnityEngine.UI.Button testAuthButton;
        public UnityEngine.UI.Button testLiveButton;
        public UnityEngine.UI.Button testChannelButton;
        public UnityEngine.UI.Text resultText;

        void Start()
        {
            if (testAuthButton != null)
                testAuthButton.onClick.AddListener(TestAuth);
            
            if (testLiveButton != null)
                testLiveButton.onClick.AddListener(TestLive);
            
            if (testChannelButton != null)
                testChannelButton.onClick.AddListener(TestChannel);
        }

        public void TestAuth()
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                UpdateResult("Error: User ID and Password are required for auth test");
                return;
            }

            UpdateResult("Testing authentication...");
            
            SoopClient.Instance.Auth.SignIn(userId, password,
                onSuccess: (cookie) =>
                {
                    UpdateResult($"Auth Success!\nAuthTicket: {cookie.AuthTicket?.Substring(0, 10)}...\nUserTicket: {cookie.UserTicket?.Substring(0, 10)}...");
                },
                onError: (error) =>
                {
                    UpdateResult($"Auth Failed: {error}");
                });
        }

        public void TestLive()
        {
            if (string.IsNullOrEmpty(streamerId))
            {
                UpdateResult("Error: Streamer ID is required for live test");
                return;
            }

            UpdateResult("Testing live API...");
            
            SoopClient.Instance.Live.GetDetail(streamerId,
                onSuccess: (liveDetail) =>
                {
                    var channel = liveDetail.CHANNEL;
                    UpdateResult($"Live Success!\nTitle: {channel.TITLE}\nViewer: {channel.CTUSER}\nBJ: {channel.BJNICK}\nResult: {channel.RESULT}");
                },
                onError: (error) =>
                {
                    UpdateResult($"Live Failed: {error}");
                });
        }

        public void TestChannel()
        {
            if (string.IsNullOrEmpty(streamerId))
            {
                UpdateResult("Error: Streamer ID is required for channel test");
                return;
            }

            UpdateResult("Testing channel API...");
            
            SoopClient.Instance.Channel.GetStation(streamerId,
                onSuccess: (stationInfo) =>
                {
                    UpdateResult($"Channel Success!\nNick: {stationInfo.station?.user_nick}\nTitle: {stationInfo.station?.station_title}\nGrade: {stationInfo.station?.grade}");
                },
                onError: (error) =>
                {
                    UpdateResult($"Channel Failed: {error}");
                });
        }

        private void UpdateResult(string result)
        {
            if (resultText != null)
                resultText.text = result;
            
            Debug.Log($"SOOP API Result: {result}");
        }
    }
}