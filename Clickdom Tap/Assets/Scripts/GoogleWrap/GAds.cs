using GoogleMobileAds.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ANU.GoogleWrap
{
    public class GAds
    {
        private class StringValueAttribute : Attribute
        {
            public string Value { get; }

            public StringValueAttribute(string value)
            {
                Value = value;
            }
        }

        public enum RewardedAdId
        {
            [StringValue("ca-app-pub-3940256099942544/5224354917")] Test,
            [StringValue("ca-app-pub-3678745180251733/2603393055")] StartSilverPack,
            [StringValue("ca-app-pub-3678745180251733/1915684268")] DoubleSilverGain,
            [StringValue("ca-app-pub-3678745180251733/5713945580")] DoubleDamage
        }

        public string ToStringId(RewardedAdId id)
        {
            var field = id
                .GetType()
                .GetMember(id.ToString())
                .Where(m => m != null)
                .FirstOrDefault();

            if(field != null)
            {
                var value = field.GetCustomAttributes(typeof(StringValueAttribute), false)
                     .Select(a => a as StringValueAttribute)
                     .Where(a => a != null)
                     .Select(a => a.Value)
                     .FirstOrDefault();

                if (value != null)
                    return value;
            }

            throw new Exception($"no string value for this {nameof(id)}");
        }

        public static GAds Instance { get; } = new GAds();

        private bool debugMode;
        private bool inited = false;

        private Dictionary<RewardedAdId, List<RewardedAd>> revarderAds = new Dictionary<RewardedAdId, List<RewardedAd>>();

        private GAds() { }

        public void Init(bool debugMode = true, Action onInit = null, bool forceInit = false)
        {
            Debug.Log("===================== GAds init");

            if (inited && !forceInit)
                return;
           
            this.debugMode = debugMode;
            MobileAds.Initialize((status) =>
            {
                onInit?.Invoke();
                inited = true;
            });
        }

        public RewardedAdId GetActualId(RewardedAdId id)
        {
            if (debugMode)
                return RewardedAdId.Test;
            else
                return id;
        }

        public void LoadRevardedAd(RewardedAdId id, Action<bool> onLoaded)
        {
            var strActualAdId = ToStringId(GetActualId(id));

            Debug.Log($"LoadRevardedAd: {id} [{strActualAdId}]");

            var ad = new RewardedAd(strActualAdId);
            var request = new AdRequest.Builder()
                            .Build();
            
            ad.OnAdLoaded += (sender, args) =>
            {
                CacheAd(id, ad);
                onLoaded?.Invoke(true);
            };
            ad.OnAdFailedToLoad += (sender, args) =>
            {
                Debug.Log($"OnAdFailedToLoad: {id}, {args.Message}");

                ClearCachedAd(id, ad);
                onLoaded?.Invoke(false);
            };

            ad.LoadAd(request);
        }

        public void ShowRewardedAd(RewardedAdId id, Action onEarnedreward,  Action onOpened, Action onClosed, Action onFailedToShow)
        {
            Debug.Log($"ShowRewardedAd: {id}");

            var ad = GetCachedAd(id);

            if(ad == null)
            {
                onFailedToShow?.Invoke();
                return;
            }

            ad.OnUserEarnedReward += (sender, args) =>
            {
                onEarnedreward?.Invoke();
            };
            ad.OnAdOpening += (sender, args) =>
            {
                onOpened?.Invoke();
            };
            ad.OnAdClosed += (sender, args) =>
            {
                ClearCachedAd(id, ad);
                onClosed?.Invoke();
            };
            ad.OnAdFailedToShow += (sender, args) =>
            {
                ClearCachedAd(id, ad);
                onFailedToShow?.Invoke();
            };

            ad.Show();
        }

        public bool HasCachedAd(RewardedAdId id)
        {
            return revarderAds.ContainsKey(id) && revarderAds[id].Any();
        }

        private void CacheAd(RewardedAdId id, RewardedAd ad)
        {
            if (!revarderAds.ContainsKey(id))
                revarderAds.Add(id, new List<RewardedAd>() { ad });
            else
                revarderAds[id].Add(ad);
        }

        private void ClearCachedAd(RewardedAdId id, RewardedAd ad)
        {
            if (revarderAds.ContainsKey(id))
                revarderAds[id].Remove(ad);
        }

        private void ClearCachedAds(RewardedAdId id)
        {
            if (revarderAds.ContainsKey(id))
                revarderAds[id].Clear();
        }

        private RewardedAd GetCachedAd(RewardedAdId id)
        {
            RewardedAd ad = null;
            if (revarderAds.ContainsKey(id))
                ad = revarderAds[id].Where(_ad => _ad.IsLoaded()).FirstOrDefault();
            return ad;
        }

    }
}
