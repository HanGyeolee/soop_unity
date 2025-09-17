using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SoopExtension
{
    [Serializable]
    public class Cookie
    {
        public string AbroadChk;
        public string AbroadVod;
        public string AuthTicket;
        public string BbsTicket;
        public string RDB;
        public string UserTicket;
        public string _au;
        public string _au3rd;
        public string _ausa;
        public string _ausb;
        public int isBbs;
    }

    public class SoopAuth
    {
        private readonly SoopClient client;

        public SoopAuth(SoopClient client)
        {
            this.client = client;
        }

        public void SignIn(string userId, string password, Action<Cookie> onSuccess, Action<string> onError, string baseUrl = null)
        {
            if (string.IsNullOrEmpty(baseUrl))
                baseUrl = client.Options.baseUrls.soopAuthBaseUrl;

            client.StartCoroutine(SignInCoroutine(userId, password, baseUrl, onSuccess, onError));
        }

        private IEnumerator SignInCoroutine(string userId, string password, string baseUrl, Action<Cookie> onSuccess, Action<string> onError)
        {
            var form = new WWWForm();
            form.AddField("szWork", "login");
            form.AddField("szType", "json");
            form.AddField("szUid", userId);
            form.AddField("szPassword", password);

            using (var request = UnityWebRequest.Post($"{baseUrl}/app/LoginAction.php", form))
            {
                request.SetRequestHeader("User-Agent", client.Options.userAgent);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"Network Error: {request.error}");
                    yield break;
                }

                var setCookieHeader = request.GetResponseHeader("set-cookie");
                if (string.IsNullOrEmpty(setCookieHeader))
                {
                    onError?.Invoke("No set-cookie header found in response");
                    yield break;
                }

                try
                {
                    var cookie = ParseCookie(setCookieHeader);
                    onSuccess?.Invoke(cookie);
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"Cookie parsing error: {ex.Message}");
                }
            }
        }

        private Cookie ParseCookie(string setCookieHeader)
        {
            var cookie = new Cookie();

            cookie.AbroadChk = GetCookieValue("AbroadChk", setCookieHeader);
            cookie.AbroadVod = GetCookieValue("AbroadVod", setCookieHeader);
            cookie.AuthTicket = GetCookieValue("AuthTicket", setCookieHeader);
            cookie.BbsTicket = GetCookieValue("BbsTicket", setCookieHeader);
            cookie.RDB = GetCookieValue("RDB", setCookieHeader);
            cookie.UserTicket = GetCookieValue("UserTicket", setCookieHeader);
            cookie._au = GetCookieValue("_au", setCookieHeader);
            cookie._au3rd = GetCookieValue("_au3rd", setCookieHeader);
            cookie._ausa = GetCookieValue("_ausa", setCookieHeader);
            cookie._ausb = GetCookieValue("_ausb", setCookieHeader);

            var isBbsStr = GetCookieValue("isBbs", setCookieHeader);
            if (int.TryParse(isBbsStr, out int isBbs))
                cookie.isBbs = isBbs;

            return cookie;
        }

        private string GetCookieValue(string name, string setCookieHeader)
        {
            var pattern = $"{name}=([^;]+)";
            var match = System.Text.RegularExpressions.Regex.Match(setCookieHeader, pattern);
            if (!match.Success)
                throw new Exception($"Cookie '{name}' not found in set-cookie header");
            return match.Groups[1].Value;
        }
    }
}
