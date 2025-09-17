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

        // API 모듈들
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

            // 기본값 설정
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
            // API 모듈 초기화
            Auth = new SoopAuth(this);
            Live = new SoopLive(this);
            Channel = new SoopChannel(this);
        }

        // HTTP 요청을 위한 기본 메서드
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

            // try-catch를 yield return 밖에서 처리
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

                // 기본 헤더 설정
                request.SetRequestHeader("User-Agent", Options.userAgent);

                // 추가 헤더 설정
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

            // 에러가 있으면 즉시 반환
            if (hasError)
            {
                onError?.Invoke(errorMessage);
                yield break;
            }

            // 요청 실행
            yield return request.SendWebRequest();

            // 결과 처리
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

            // 콜백 호출
            if (hasError)
            {
                onError?.Invoke(errorMessage);
            }
            else
            {
                onSuccess?.Invoke(resultText);
            }
        }

        // 채팅 생성 메서드
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
                // NativeWebSocket의 메시지 처리를 위해 필요
                NativeWebSocket.WebSocket.DispatchMessageQueue();
            }
            catch (System.Exception)
            {
                // NativeWebSocket이 없으면 무시
            }
        }
    }
}
