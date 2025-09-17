using UnityEngine;
using SoopExtension;

namespace SoopExtension.Samples
{
    public class SoopChatExample : MonoBehaviour
    {
        [Header("SOOP Settings")]
        public string streamerId = "your_streamer_id";
        public string userId = "your_user_id"; // 로그인용 (선택사항)
        public string password = "your_password"; // 로그인용 (선택사항)

        [Header("UI References")]
        public UnityEngine.UI.Button connectButton;
        public UnityEngine.UI.Button disconnectButton;
        public UnityEngine.UI.Text statusText;
        public UnityEngine.UI.Text chatLogText;
        public UnityEngine.UI.ScrollRect chatScrollRect;

        private SoopChat chat;
        private string chatLog = "";

        void Start()
        {
            // UI 버튼 이벤트 연결
            if (connectButton != null)
                connectButton.onClick.AddListener(ConnectToChat);
            
            if (disconnectButton != null)
                disconnectButton.onClick.AddListener(DisconnectFromChat);

            UpdateStatus("Ready to connect");
        }

        public void ConnectToChat()
        {
            if (string.IsNullOrEmpty(streamerId))
            {
                UpdateStatus("Error: Streamer ID is required");
                return;
            }

            var chatOptions = new SoopChatOptions
            {
                streamerId = streamerId
            };

            // 로그인 정보가 있으면 추가
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(password))
            {
                chatOptions.login = new SoopLoginOptions
                {
                    userId = userId,
                    password = password
                };
            }

            chat = SoopClient.Instance.CreateChat(chatOptions);
            SetupChatEvents();
            
            UpdateStatus("Connecting...");
            chat.Connect();
        }

        public void DisconnectFromChat()
        {
            if (chat != null)
            {
                chat.Disconnect();
                chat = null;
            }
            UpdateStatus("Disconnected");
        }

        private void SetupChatEvents()
        {
            // 연결 성공
            chat.On<ConnectResponse>(SoopChatEvent.CONNECT, response =>
            {
                string username = !string.IsNullOrEmpty(response.username) ? response.username : "Anonymous";
                UpdateStatus($"Connected as {username}");
                AddChatLog($"[SYSTEM] Connected to {response.streamerId}");
            });

            // 채팅방 입장
            chat.On<EnterChatRoomResponse>(SoopChatEvent.ENTER_CHAT_ROOM, response =>
            {
                AddChatLog($"[SYSTEM] Entered chat room: {response.streamerId}");
            });

            // 일반 채팅
            chat.On<ChatMessageResponse>(SoopChatEvent.CHAT, response =>
            {
                AddChatLog($"{response.username}: {response.comment}");
            });

            // 후원 채팅
            chat.On<DonationResponse>(SoopChatEvent.TEXT_DONATION, response =>
            {
                AddChatLog($"[DONATION] {response.fromUsername} donated {response.amount} to {response.to}");
            });

            // 구독
            chat.On<SubscribeResponse>(SoopChatEvent.SUBSCRIBE, response =>
            {
                AddChatLog($"[SUBSCRIBE] {response.fromUsername} subscribed for {response.monthCount} months (Tier {response.tier})");
            });

            // 이모티콘
            chat.On<EmoticonResponse>(SoopChatEvent.EMOTICON, response =>
            {
                AddChatLog($"[EMOTICON] {response.username}: {response.emoticonId}");
            });

            // 공지사항
            chat.On<NotificationResponse>(SoopChatEvent.NOTIFICATION, response =>
            {
                AddChatLog($"[NOTICE] {response.notification}");
            });

            // 입장/퇴장
            chat.On<ViewerResponse>(SoopChatEvent.VIEWER, response =>
            {
                if (response.userId.Length == 1)
                    AddChatLog($"[SYSTEM] {response.userId[0]} entered");
            });

            chat.On<ExitResponse>(SoopChatEvent.EXIT, response =>
            {
                AddChatLog($"[SYSTEM] {response.username} left");
            });

            // 연결 해제
            chat.On<DisconnectResponse>(SoopChatEvent.DISCONNECT, response =>
            {
                UpdateStatus("Disconnected - Stream ended");
                AddChatLog("[SYSTEM] Stream ended");
            });
        }

        private void UpdateStatus(string status)
        {
            if (statusText != null)
                statusText.text = $"Status: {status}";
            
            Debug.Log($"SOOP Chat Status: {status}");
        }

        private void AddChatLog(string message)
        {
            chatLog += $"[{System.DateTime.Now:HH:mm:ss}] {message}\n";
            
            if (chatLogText != null)
            {
                chatLogText.text = chatLog;
                
                // 자동 스크롤
                if (chatScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    chatScrollRect.verticalNormalizedPosition = 0f;
                }
            }
            
            Debug.Log($"SOOP Chat: {message}");
        }

        void OnDestroy()
        {
            DisconnectFromChat();
        }
    }
}