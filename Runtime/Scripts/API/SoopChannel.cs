using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace SoopExtension
{
    [Serializable]
    public class StationInfo
    {
        public string profile_image;
        public Station station;
        public Broad broad;
        public StarBalloonTop[] starballoon_top;
        public StickerTop[] sticker_top;
        public Subscription subscription;
        public bool is_best_bj;
        public bool is_partner_bj;
        public bool is_ppv_bj;
        public bool is_af_supporters_bj;
        public bool is_shopfreeca_bj;
        public bool is_favorite;
        public bool is_subscription;
        public bool is_owner;
        public bool is_manager;
        public bool is_notice;
        public bool is_adsence;
        public bool is_mobile_push;
        public string subscribe_visible;
        public string country;
        public string current_timestamp;
    }

    [Serializable]
    public class Station
    {
        public string broad_start;
        public int grade;
        public string jointime;
        public string station_name;
        public int station_no;
        public string station_title;
        public int total_broad_time;
        public string user_id;
        public string user_nick;
        public int active_no;
    }

    [Serializable]
    public class Broad
    {
        public string user_id;
        public int broad_no;
        public string broad_title;
        public int current_sum_viewer;
        public int broad_grade;
        public bool is_password;
    }

    [Serializable]
    public class StarBalloonTop
    {
        public string user_id;
        public string user_nick;
        public string profile_image;
    }

    [Serializable]
    public class StickerTop
    {
        public string user_id;
        public string user_nick;
        public string profile_image;
    }

    [Serializable]
    public class Subscription
    {
        public int total;
        public int tier1;
        public int tier2;
    }

    public class SoopChannel
    {
        private readonly SoopClient client;

        public SoopChannel(SoopClient client)
        {
            this.client = client;
        }

        public void GetStation(string streamerId, Action<StationInfo> onSuccess, Action<string> onError, string baseUrl = null)
        {
            if (string.IsNullOrEmpty(baseUrl))
                baseUrl = client.Options.baseUrls.soopChannelBaseUrl;

            client.StartCoroutine(GetStationCoroutine(streamerId, baseUrl, onSuccess, onError));
        }

        private IEnumerator GetStationCoroutine(string streamerId, string baseUrl, Action<StationInfo> onSuccess, Action<string> onError)
        {
            using (var request = UnityWebRequest.Get($"{baseUrl}/api/{streamerId}/station"))
            {
                request.SetRequestHeader("User-Agent", client.Options.userAgent);

                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    onError?.Invoke($"Network Error: {request.error}");
                    yield break;
                }

                try
                {
                    var stationInfo = JsonUtility.FromJson<StationInfo>(request.downloadHandler.text);
                    onSuccess?.Invoke(stationInfo);
                }
                catch (Exception ex)
                {
                    onError?.Invoke($"JSON parsing error: {ex.Message}");
                }
            }
        }
    }
}
