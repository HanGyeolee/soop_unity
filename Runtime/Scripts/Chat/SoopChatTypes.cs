using System;

namespace SoopExtension
{
    public static class ChatDelimiter
    {
        public static readonly string STARTER = "\x1b\t";  // TypeScript 원본과 일치
        public static readonly char SEPARATOR = '\x0c';
        public static readonly char ELEMENT_START = '\x11';
        public static readonly char ELEMENT_END = '\x12';
        public static readonly char SPACE = '\x06';
    }

    public enum ChatType
    {
        PING = 0,
        CONNECT = 1,
        ENTER_CHAT_ROOM = 2,
        EXIT = 4,
        CHAT = 5,
        DISCONNECT = 7,
        ENTER_INFO = 12,
        TEXT_DONATION = 18,
        AD_BALLOON_DONATION = 87,
        SUBSCRIBE = 93,
        NOTIFICATION = 104,
        VIDEO_DONATION = 105,
        EMOTICON = 109,
        VIEWER = 127
    }

    public enum SoopChatEvent
    {
        CONNECT,
        ENTER_CHAT_ROOM,
        DISCONNECT,
        CHAT,
        EMOTICON,
        NOTIFICATION,
        TEXT_DONATION,
        VIDEO_DONATION,
        AD_BALLOON_DONATION,
        SUBSCRIBE,
        VIEWER,
        EXIT,
        UNKNOWN,
        RAW
    }

    [Serializable]
    public class ChatResponse
    {
        public string receivedTime;
    }

    [Serializable]
    public class ConnectResponse : ChatResponse
    {
        public string syn;
        public string username;
        public string streamerId;
    }

    [Serializable]
    public class EnterChatRoomResponse : ChatResponse
    {
        public string synAck;
        public string streamerId;
    }

    [Serializable]
    public class NotificationResponse : ChatResponse
    {
        public string notification;
    }

    [Serializable]
    public class ChatMessageResponse : ChatResponse
    {
        public string username;
        public string userId;
        public string comment;
    }

    [Serializable]
    public class DonationResponse : ChatResponse
    {
        public string to;
        public string from;
        public string fromUsername;
        public string amount;
        public string fanClubOrdinal;
    }

    [Serializable]
    public class EmoticonResponse : ChatResponse
    {
        public string userId;
        public string username;
        public string emoticonId;
    }

    [Serializable]
    public class ViewerResponse : ChatResponse
    {
        public string[] userId;
    }

    [Serializable]
    public class SubscribeResponse : ChatResponse
    {
        public string to;
        public string from;
        public string fromUsername;
        public string monthCount;
        public string tier;
    }

    [Serializable]
    public class ExitResponse : ChatResponse
    {
        public string username;
        public string userId;
    }

    [Serializable]
    public class DisconnectResponse : ChatResponse
    {
        public string streamerId;
    }
}
