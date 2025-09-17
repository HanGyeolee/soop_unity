using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;

namespace SoopExtension
{
    public class SoopClient : MonoBehaviour
    {
        public readonly SoopClientOptions Options;

        // API ����
        public SoopAuth Auth { get; private set; }
        public SoopLive Live { get; private set; }
        public SoopChannel Channel { get; private set; }

        private static SoopClient instance;
        public static SoopClient Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("SoopClient");
                    instance = go.AddComponent<SoopClient>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        public SoopClient() : this(new SoopClientOptions()) { }

        public SoopClient(SoopClientOptions options)
        {
            Options = options ?? new SoopClientOptions();

            // �⺻�� ����
            if (Options.baseUrls == null)
                Options.baseUrls = SoopConstants.DEFAULT_BASE_URLS;

            if (string.IsNullOrEmpty(Options.userAgent))
                Options.userAgent = SoopConstants.DEFAULT_USER_AGENT;

            InitializeAPIModules();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAPIModules();
        }

        private void InitializeAPIModules()
        {
            // API ��� �ʱ�ȭ
            Auth = new SoopAuth(this);
            Live = new SoopLive(this);
            Channel = new SoopChannel(this);
        }

        // HTTP ��û�� ���� �⺻ �޼���
        public void SendWebRequest(string url, System.Action<string> onSuccess, System.Action<string> onError,
            string method = "GET", string postData = null, Dictionary<string, string> headers = null)
        {
            StartCoroutine(SendWebRequestCoroutine(url, onSuccess, onError, method, postData, headers));
        }

        private IEnumerator SendWebRequestCoroutine(string url, System.Action<string> onSuccess, System.Action<string> onError,
            string method, string postData, Dictionary<string, string> headers)
        {
            UnityWebRequest request = null;
            bool hasError = false;
            string errorMessage = "";
            string resultText = "";

            // try-catch�� yield return �ۿ��� ó��
            try
            {
                if (method.ToUpper() == "GET")
                {
                    request = UnityWebRequest.Get(url);
                }
                else if (method.ToUpper() == "POST")
                {
                    request = UnityWebRequest.Post(url, postData);
                }
                else
                {
                    request = new UnityWebRequest(url, method);
                    if (!string.IsNullOrEmpty(postData))
                    {
                        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
                        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    }
                    request.downloadHandler = new DownloadHandlerBuffer();
                }

                // �⺻ ��� ����
                request.SetRequestHeader("User-Agent", Options.userAgent);

                // �߰� ��� ����
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        request.SetRequestHeader(header.Key, header.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = $"Request setup failed: {ex.Message}";
            }

            // ������ ������ ��� ��ȯ
            if (hasError)
            {
                onError?.Invoke(errorMessage);
                yield break;
            }

            // ��û ����
            yield return request.SendWebRequest();

            // ��� ó��
            try
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    resultText = request.downloadHandler.text;
                }
                else
                {
                    hasError = true;
                    errorMessage = $"Request failed: {request.error}";
                }
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = $"Response processing failed: {ex.Message}";
            }
            finally
            {
                request?.Dispose();
            }

            // �ݹ� ȣ��
            if (hasError)
            {
                onError?.Invoke(errorMessage);
            }
            else
            {
                onSuccess?.Invoke(resultText);
            }
        }

        // ä�� ���� �޼���
        public SoopChat CreateChat(SoopChatOptions options)
        {
            if (options.baseUrls == null)
                options.baseUrls = Options.baseUrls;

            return new SoopChat(this, options);
        }

        private void Update()
        {
            try
            {
                // NativeWebSocket�� �޽��� ó���� ���� �ʿ�
                NativeWebSocket.WebSocket.DispatchMessageQueue();
            }
            catch (System.Exception)
            {
                // NativeWebSocket�� ������ ����
            }
        }
    }
}
