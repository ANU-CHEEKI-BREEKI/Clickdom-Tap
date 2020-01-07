using UnityEngine;
using System.Collections;
using ANU.GoogleWrap;
using UnityEngine.Events;

public class RewardedAdAdapter : MonoBehaviour
{
    [System.Serializable]
    public class BoolUnityEvent : UnityEvent<bool>{}

    [SerializeField] public GAds.RewardedAdId adId;
    [SerializeField] private AUserReward reward;
    [SerializeField] private BoolUnityEvent OnRewardAvailable;

    private void Start()
    {
        OnRewardAvailable?.Invoke(GAds.Instance.HasCachedAd(adId));
    }

    public void Init()
    {
        print("Init");
        OnRewardAvailable?.Invoke(GAds.Instance.HasCachedAd(adId));
        LoadAd();
    }

    public void ShowAd()
    {
        print("ShowAd");

        var mtd = MainThreadDispatcher.Instance;

        GAds.Instance.ShowRewardedAd(
            adId,
            () => mtd.Enqueue(() => reward?.EarnReward()),
            () => mtd.Enqueue(() => PauseManager.Instance?.FullPauseGame()),
            () => mtd.Enqueue(() => OnCloseOrFailed()),
            () => mtd.Enqueue(() => OnCloseOrFailed())
        );
    }

    private void OnCloseOrFailed()
    {
        OnRewardAvailable?.Invoke(GAds.Instance.HasCachedAd(adId));
        LoadAd();
    }

    private void LoadAd()
    {
        print("LoadAd");

        var mtd = MainThreadDispatcher.Instance;

        GAds.Instance.LoadRevardedAd(
            adId,
            (success) => mtd.Enqueue(
                () => {
                    Debug.Log($"RevardedAdLoaded: {success}");
                    OnRewardAvailable?.Invoke(GAds.Instance.HasCachedAd(adId));
                }
            )
        );
    }
}
