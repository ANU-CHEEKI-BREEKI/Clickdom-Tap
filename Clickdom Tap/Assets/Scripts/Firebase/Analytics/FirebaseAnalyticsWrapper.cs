using Firebase;
using Firebase.Analytics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FirebaseAnalyticsWrapper
{
    public static bool Initialited => Instance != null;
    public static DependencyStatus Status { get; private set; } = DependencyStatus.UnavailableOther;
    public static event Action<bool> OnInitialized;

    public static FirebaseApp Instance { get; private set; } = null;

    public static void Init()
    {
        if (Initialited)
            return;

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            Status = task.Result;

            if (Status == DependencyStatus.Available)
                Instance = FirebaseApp.DefaultInstance;
            else
                Debug.LogError($"Could not resolve all Firebase dependencies: {Status}");

            OnInitialized?.Invoke(Initialited);
        });
    }

    /// <summary>
    /// Saving data to remote store (for example to GoogleServices)
    /// </summary>
    /// <param name="bytesWritten">written bytes count</param>
    public static void LogRemoteSavedDataEvent(int bytesWritten, string storeName = "")
    {
        if (!Initialited)
            return;

        FirebaseAnalytics.LogEvent(
            "Save data remote operation",
            new Parameter[]
            {
                new Parameter("Bytes written", bytesWritten),
                new Parameter("Store name", storeName)
            }
        );
    }

    /// <summary>
    /// Loading data from remote store (for example from GoogleServices)
    /// </summary>
    /// <param name="bytesRead">read bytes count</param>
    public static void LogRemoteLoadedDataEvent(int bytesRead, string storeName = "")
    {
        if (!Initialited)
            return;

        FirebaseAnalytics.LogEvent(
            "Load data remote operation",
            new Parameter[]
            {
                new Parameter("Bytes read", bytesRead),
                new Parameter("Store name", storeName)
            }
        );
    }

    /// <summary>
    /// Saving data to local store (for example to PlayerPrefs)
    /// </summary>
    /// <param name="bytesWritten">written bytes count</param>
    public static void LogLocalSavedDataEvent(int bytesWritten, string storeName = "")
    {
        if (!Initialited)
            return;

        FirebaseAnalytics.LogEvent(
            "Save data local operation",
            new Parameter[]
            {
                new Parameter("Bytes written", bytesWritten),
                new Parameter("Store name", storeName)
            }
        );
    }

    /// <summary>
    /// Loading data from local store (for example from PlayerPrefs)
    /// </summary>
    /// <param name="bytesRead">read bytes count</param>
    public static void LogLocalLoadedDataEvent(int bytesRead, string storeName = "")
    {
        if (!Initialited)
            return;

        FirebaseAnalytics.LogEvent(
            "Load data local operation",
            new Parameter[]
            {
                new Parameter("Bytes read", bytesRead),
                new Parameter("Store name", storeName)
            }
        );
    }

    /// <summary>
    /// Some frequently used events preset. Just call .LogEvent() on it.
    /// </summary>
    public static class CustomEvents
    {
        //public static readonly FirebaseAnalyticsCustomEvent SimpleSaveDataEvent = new FirebaseAnalyticsCustomEvent()
        //{
        //    Name = "Save data operation"
        //};

        //public static readonly FirebaseAnalyticsCustomEvent SimpleLoadDataEvent = new FirebaseAnalyticsCustomEvent()
        //{
        //    Name = "Load data operation"
        //};
    }
}
