using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SoopExtension
{
    [Serializable]
    public class LiveDetail
    {
        public Channel CHANNEL;
    }

    [Serializable]
    public class Channel
    {
        public string geo_cc;
        public string geo_rc;
        public string acpt_lang;
        public string svc_lang;
        public int ISSP;
        public int LOWLAYTENCYBJ;
        public ViewPreset[] VIEWPRESET;
        public int RESULT;
        public string PBNO;
        public string BNO;
        public string BJID;
        public string BJNICK;
        public int BJGRADE;
        public string STNO;
        public string ISFAV;
        public string CATE;
        public int CPNO;
        public string GRADE;
        public string BTYPE;
        public string CHATNO;
        public string BPWD;
        public string TITLE;
        public string BPS;
        public string RESOLUTION;
        public string CTIP;
        public string CTPT;
        public string VBT;
        public int CTUSER;
        public int S1440P;
        public string[] AUTO_HASHTAGS;
        public string[] CATEGORY_TAGS;
        public string[] HASH_TAGS;
        public string CHIP;
        public string CHPT;
        public string CHDOMAIN;
        public string CDN;
        public string RMD;
        public string GWIP;
        public string GWPT;
        public string STYPE;
        public string ORG;
        public string MDPT;
        public int BTIME;
        public int DH;
        public int WC;
        public int PCON;
        public int PCON_TIME;
        public string[] PCON_MONTH;
        public string FTK;
        public bool BPCBANNER;
        public bool BPCCHATPOPBANNER;
        public bool BPCTIMEBANNER;
        public bool BPCCONNECTBANNER;
        public bool BPCLOADINGBANNER;
        public string BPCPOSTROLL;
        public string BPCPREROLL;
        public string TIER1_NICK;
        public string TIER2_NICK;
        public int EXPOSE_FLAG;
        public int SUB_PAY_CNT;
    }

    [Serializable]
    public class ViewPreset
    {
        public string label;
        public string label_resolution;
        public string name;
        public int bps;
    }

    [Serializable]
    public class LiveDetailOptions
    {
        public string type = "live";
        public string pwd = "";
        public string player_type = "html5";
        public string stream_type = "common";
        public string quality = "HD";
        public string mode = "landing";
        public string from_api = "0";
        public bool is_revive = false;
    }

    public class SoopLive
    {
        private readonly SoopClient client;

        public SoopLive(SoopClient client)
        {
            this.client = client;
        }

        public void GetDetail(string streamerId, Action<LiveDetail> onSuccess, Action<string> onError, Cookie cookie = null, LiveDetailOptions options = null, string baseUrl = null)
        {
            if (string.IsNullOrEmpty(baseUrl))
                baseUrl = client.Options.baseUrls.soopLiveBaseUrl;

            if (options == null)
                options = new LiveDetailOptions();

            client.StartCoroutine(GetDetailCoroutine(streamerId, baseUrl, cookie, options, onSuccess, onError));
        }

        private IEnumerator GetDetailCoroutine(string streamerId, string baseUrl, Cookie cookie, LiveDetailOptions options, Action<LiveDetail> onSuccess, Action<string> onError)
        {
            var form = new WWWForm();
            form.AddField("bid", streamerId);
            form.AddField("type", options.type);
            form.AddField("pwd", options.pwd);
            form.AddField("player_type", options.player_type);
            form.AddField("stream_type", options.stream_type);
            form.AddField("quality", options.quality);
            form.AddField("mode", options.mode);
            form.AddField("from_api", options.from_api);
            form.AddField("is_revive", options.is_revive ? "true" : "false");

            using (var request = UnityWebRequest.Post($"{baseUrl}/afreeca/player_live_api.php?bjid={streamerId}", form))
            {
                request.SetRequestHeader("User-Agent", client.Options.userAgent);
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                if (cookie != null)
                {
                    request.SetRequestHeader("Cookie", BuildCookieString(cookie));
                }

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"Network Error: {request.error}");
                    yield break;
                }

                try
                {
                    var liveDetail = JsonUtility.FromJson<LiveDetail>($"{{\"CHANNEL\":{request.downloadHandler.text}}}");
                    onSuccess?.Invoke(liveDetail);
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"JSON parsing error: {ex.Message}");
                }
            }
        }

        private string BuildCookieString(Cookie cookie)
        {
            var cookies = new List<string>();

            if (!string.IsNullOrEmpty(cookie.AbroadChk)) cookies.Add($"AbroadChk={cookie.AbroadChk}");
            if (!string.IsNullOrEmpty(cookie.AbroadVod)) cookies.Add($"AbroadVod={cookie.AbroadVod}");
            if (!string.IsNullOrEmpty(cookie.AuthTicket)) cookies.Add($"AuthTicket={cookie.AuthTicket}");
            if (!string.IsNullOrEmpty(cookie.BbsTicket)) cookies.Add($"BbsTicket={cookie.BbsTicket}");
            if (!string.IsNullOrEmpty(cookie.RDB)) cookies.Add($"RDB={cookie.RDB}");
            if (!string.IsNullOrEmpty(cookie.UserTicket)) cookies.Add($"UserTicket={cookie.UserTicket}");
            if (!string.IsNullOrEmpty(cookie._au)) cookies.Add($"_au={cookie._au}");
            if (!string.IsNullOrEmpty(cookie._au3rd)) cookies.Add($"_au3rd={cookie._au3rd}");
            if (!string.IsNullOrEmpty(cookie._ausa)) cookies.Add($"_ausa={cookie._ausa}");
            if (!string.IsNullOrEmpty(cookie._ausb)) cookies.Add($"_ausb={cookie._ausb}");
            cookies.Add($"isBbs={cookie.isBbs}");

            return string.Join("; ", cookies);
        }
    }
}
