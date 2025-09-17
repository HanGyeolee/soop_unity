using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NativeWebSocket;
using UnityEngine;

namespace SoopExtension
{
    public class SoopChat
    {
        private readonly SoopClient client;
        private readonly SoopChatOptions options;
        private WebSocket websocket;
        private LiveDetail liveDetail;
        private Cookie cookie;

        private bool isConnected = false;
        private bool isEntered = false;
        private Coroutine pingCoroutine;

        // 이벤트 핸들러들
        private readonly Dictionary<SoopChatEvent, System.Action<object>> eventHandlers = new Dictionary<SoopChatEvent, System.Action<object>>();

        public SoopChat(SoopClient client, SoopChatOptions options)
        {
            this.client = client;
            this.options = options;
        }

        public void On<T>(SoopChatEvent eventType, System.Action<T> handler) where T : ChatResponse
        {
            if (!eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType] = null;

            eventHandlers[eventType] += (obj) => handler((T)obj);
        }

        public async void Connect()
        {
            if (isConnected)
            {
                Debug.LogError("Already connected");
                return;
            }

            try
            {
                // 로그인이 설정되어 있으면 인증 진행
                if (options.login != null && !string.IsNullOrEmpty(options.login.userId))
                {
                    await GetCookieAsync();
                }

                await GetLiveDetailAsync();

                if (liveDetail.CHANNEL.RESULT == 0)
                {
                    throw new Exception("Not Streaming now");
                }

                string chatUrl = MakeChatUrl(liveDetail);
                websocket = new WebSocket(chatUrl, new Dictionary<string, string>
                {
                    {"Sec-WebSocket-Protocol", "chat"}
                });

                websocket.OnOpen += OnWebSocketOpen;
                websocket.OnMessage += OnWebSocketMessage;
                websocket.OnClose += OnWebSocketClose;
                websocket.OnError += OnWebSocketError;

                await websocket.Connect();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection failed: {ex.Message}");
            }
        }

        public async void Disconnect()
        {
            if (!isConnected) return;

            var response = new DisconnectResponse
            {
                streamerId = options.streamerId,
                receivedTime = DateTime.Now.ToString("O")
            };
            EmitEvent(SoopChatEvent.DISCONNECT, response);

            if (pingCoroutine != null)
            {
                client.StopCoroutine(pingCoroutine);
                pingCoroutine = null;
            }

            if (websocket != null)
            {
                await websocket.Close();
                websocket = null;
            }

            isConnected = false;
            isEntered = false;

            // SoopClient에서 이 인스턴스 제거
            client.RemoveChatInstance(this);
        }

        private System.Threading.Tasks.Task GetCookieAsync()
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();

            client.Auth.SignIn(options.login.userId, options.login.password,
                onSuccess: (cookieResult) =>
                {
                    cookie = cookieResult;
                    tcs.SetResult(true);
                },
                onError: (error) => tcs.SetException(new Exception(error)));

            return tcs.Task;
        }

        private System.Threading.Tasks.Task GetLiveDetailAsync()
        {
            var tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();

            client.Live.GetDetail(options.streamerId,
                onSuccess: (detail) =>
                {
                    liveDetail = detail;
                    tcs.SetResult(true);
                },
                onError: (error) => tcs.SetException(new Exception(error)),
                cookie: cookie);

            return tcs.Task;
        }

        private void OnWebSocketOpen()
        {
            var connectPacket = GetConnectPacket();
            websocket.Send(connectPacket);
        }

        private void OnWebSocketMessage(byte[] data)
        {
            var receivedTime = DateTime.Now.ToString("O");
            var packet = Encoding.UTF8.GetString(data);

            // RAW 이벤트 발생
            EmitEvent(SoopChatEvent.RAW, packet);

            try
            {
                var messageType = ParseMessageType(packet);
                ProcessMessage(messageType, packet, receivedTime);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Message processing error: {ex.Message}");
                EmitEvent(SoopChatEvent.UNKNOWN, packet);
            }
        }

        private void OnWebSocketClose(WebSocketCloseCode closeCode)
        {
            client.StartCoroutine(DisconnectCoroutine());
        }

        private void OnWebSocketError(string error)
        {
            Debug.LogError($"WebSocket Error: {error}");
        }

        private IEnumerator DisconnectCoroutine()
        {
            yield return null;
            Disconnect();
        }

        private ChatType ParseMessageType(string packet)
        {
            if (packet.Length < 6 || packet[0] != (char)ChatDelimiter.STARTER)
                throw new Exception("Invalid packet format");

            var typeStr = packet.Substring(2, 4);
            if (int.TryParse(typeStr, out int typeInt))
                return (ChatType)typeInt;

            throw new Exception("Invalid message type");
        }

        private void ProcessMessage(ChatType messageType, string packet, string receivedTime)
        {
            var parts = packet.Split((char)ChatDelimiter.SEPARATOR);

            switch (messageType)
            {
                case ChatType.CONNECT:
                    isConnected = true;
                    var connectResponse = new ConnectResponse
                    {
                        receivedTime = receivedTime,
                        username = parts.Length > 1 ? parts[1] : "",
                        syn = parts.Length > 2 ? parts[2] : "",
                        streamerId = options.streamerId
                    };
                    EmitEvent(SoopChatEvent.CONNECT, connectResponse);

                    var joinPacket = GetJoinPacket();
                    websocket.Send(joinPacket);
                    break;

                case ChatType.ENTER_CHAT_ROOM:
                    var enterResponse = new EnterChatRoomResponse
                    {
                        receivedTime = receivedTime,
                        streamerId = parts.Length > 2 ? parts[2] : "",
                        synAck = parts.Length > 7 ? parts[7] : ""
                    };
                    EmitEvent(SoopChatEvent.ENTER_CHAT_ROOM, enterResponse);
                    isEntered = true;
                    StartPingInterval();
                    break;

                case ChatType.CHAT:
                    var chatResponse = new ChatMessageResponse
                    {
                        receivedTime = receivedTime,
                        comment = parts.Length > 1 ? parts[1] : "",
                        userId = parts.Length > 2 ? parts[2] : "",
                        username = parts.Length > 6 ? parts[6] : ""
                    };
                    EmitEvent(SoopChatEvent.CHAT, chatResponse);
                    break;

                case ChatType.NOTIFICATION:
                    var notificationResponse = new NotificationResponse
                    {
                        receivedTime = receivedTime,
                        notification = parts.Length > 4 ? parts[4] : ""
                    };
                    EmitEvent(SoopChatEvent.NOTIFICATION, notificationResponse);
                    break;

                case ChatType.TEXT_DONATION:
                    var textDonationResponse = new DonationResponse
                    {
                        receivedTime = receivedTime,
                        to = parts.Length > 1 ? parts[1] : "",
                        from = parts.Length > 2 ? parts[2] : "",
                        fromUsername = parts.Length > 3 ? parts[3] : "",
                        amount = parts.Length > 4 ? parts[4] : "",
                        fanClubOrdinal = parts.Length > 5 ? parts[5] : ""
                    };
                    EmitEvent(SoopChatEvent.TEXT_DONATION, textDonationResponse);
                    break;

                case ChatType.VIDEO_DONATION:
                    var videoDonationResponse = new DonationResponse
                    {
                        receivedTime = receivedTime,
                        to = parts.Length > 2 ? parts[2] : "",
                        from = parts.Length > 3 ? parts[3] : "",
                        fromUsername = parts.Length > 4 ? parts[4] : "",
                        amount = parts.Length > 5 ? parts[5] : "",
                        fanClubOrdinal = parts.Length > 6 ? parts[6] : ""
                    };
                    EmitEvent(SoopChatEvent.VIDEO_DONATION, videoDonationResponse);
                    break;

                case ChatType.AD_BALLOON_DONATION:
                    var adBalloonResponse = new DonationResponse
                    {
                        receivedTime = receivedTime,
                        to = parts.Length > 2 ? parts[2] : "",
                        from = parts.Length > 3 ? parts[3] : "",
                        fromUsername = parts.Length > 4 ? parts[4] : "",
                        amount = parts.Length > 10 ? parts[10] : "",
                        fanClubOrdinal = parts.Length > 11 ? parts[11] : ""
                    };
                    EmitEvent(SoopChatEvent.AD_BALLOON_DONATION, adBalloonResponse);
                    break;

                case ChatType.EMOTICON:
                    var emoticonResponse = new EmoticonResponse
                    {
                        receivedTime = receivedTime,
                        emoticonId = parts.Length > 3 ? parts[3] : "",
                        userId = parts.Length > 6 ? parts[6] : "",
                        username = parts.Length > 7 ? parts[7] : ""
                    };
                    EmitEvent(SoopChatEvent.EMOTICON, emoticonResponse);
                    break;

                case ChatType.VIEWER:
                    var viewerResponse = new ViewerResponse
                    {
                        receivedTime = receivedTime,
                        userId = parts.Length > 4 ? GetViewerElements(parts) : new[] { parts.Length > 1 ? parts[1] : "" }
                    };
                    EmitEvent(SoopChatEvent.VIEWER, viewerResponse);
                    break;

                case ChatType.SUBSCRIBE:
                    var subscribeResponse = new SubscribeResponse
                    {
                        receivedTime = receivedTime,
                        to = parts.Length > 1 ? parts[1] : "",
                        from = parts.Length > 2 ? parts[2] : "",
                        fromUsername = parts.Length > 3 ? parts[3] : "",
                        monthCount = parts.Length > 4 ? parts[4] : "",
                        tier = parts.Length > 8 ? parts[8] : ""
                    };
                    EmitEvent(SoopChatEvent.SUBSCRIBE, subscribeResponse);
                    break;

                case ChatType.EXIT:
                    var exitResponse = new ExitResponse
                    {
                        receivedTime = receivedTime,
                        userId = parts.Length > 2 ? parts[2] : "",
                        username = parts.Length > 3 ? parts[3] : ""
                    };
                    EmitEvent(SoopChatEvent.EXIT, exitResponse);
                    break;

                case ChatType.DISCONNECT:
                    client.StartCoroutine(DisconnectCoroutine());
                    break;

                default:
                    EmitEvent(SoopChatEvent.UNKNOWN, parts);
                    break;
            }
        }

        private void EmitEvent(SoopChatEvent eventType, object data)
        {
            if (eventHandlers.ContainsKey(eventType))
                eventHandlers[eventType]?.Invoke(data);
        }

        private string[] GetViewerElements(string[] array)
        {
            var result = new List<string>();
            for (int i = 1; i < array.Length; i += 2)
            {
                result.Add(array[i]);
            }
            return result.ToArray();
        }

        private void StartPingInterval()
        {
            if (pingCoroutine != null)
                client.StopCoroutine(pingCoroutine);

            pingCoroutine = client.StartCoroutine(PingCoroutine());
        }

        private IEnumerator PingCoroutine()
        {
            while (isConnected)
            {
                yield return new WaitForSeconds(60f);
                if (websocket != null && websocket.State == WebSocketState.Open)
                {
                    var pingPacket = GetPacket(ChatType.PING, ((char)ChatDelimiter.SEPARATOR).ToString());
                    websocket.Send(Encoding.UTF8.GetBytes(pingPacket));
                }
            }
        }

        private string MakeChatUrl(LiveDetail detail)
        {
            return $"wss://{detail.CHANNEL.CHDOMAIN.ToLower()}:{int.Parse(detail.CHANNEL.CHPT) + 1}/Websocket/{options.streamerId}";
        }

        private byte[] GetConnectPacket()
        {
            string payload = $"{(char)ChatDelimiter.SEPARATOR}{(char)ChatDelimiter.SEPARATOR}{(char)ChatDelimiter.SEPARATOR}16{(char)ChatDelimiter.SEPARATOR}";
            if (cookie?.AuthTicket != null)
            {
                payload = $"{(char)ChatDelimiter.SEPARATOR}{cookie.AuthTicket}{(char)ChatDelimiter.SEPARATOR}{(char)ChatDelimiter.SEPARATOR}16{(char)ChatDelimiter.SEPARATOR}";
            }
            return Encoding.UTF8.GetBytes(GetPacket(ChatType.CONNECT, payload));
        }

        private byte[] GetJoinPacket()
        {
            string payload = $"{(char)ChatDelimiter.SEPARATOR}{liveDetail.CHANNEL.CHATNO}";

            if (cookie != null)
            {
                payload += $"{(char)ChatDelimiter.SEPARATOR}{liveDetail.CHANNEL.FTK}";
                payload += $"{(char)ChatDelimiter.SEPARATOR}0{(char)ChatDelimiter.SEPARATOR}";
                payload += $"{(char)ChatDelimiter.SEPARATOR}";
            }
            else
            {
                for (int i = 0; i < 5; i++)
                    payload += (char)ChatDelimiter.SEPARATOR;
            }

            return Encoding.UTF8.GetBytes(GetPacket(ChatType.ENTER_CHAT_ROOM, payload));
        }

        private string GetPacket(ChatType chatType, string payload)
        {
            var packetHeader = $"{(char)ChatDelimiter.STARTER}{((int)chatType):D4}{GetPayloadLength(payload):D6}00";
            return packetHeader + payload;
        }

        private int GetPayloadLength(string payload)
        {
            return Encoding.UTF8.GetByteCount(payload);
        }

        public void ProcessMessages()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            if (websocket != null)
            {
                websocket.DispatchMessageQueue();
            }
#endif
        }
    }
}