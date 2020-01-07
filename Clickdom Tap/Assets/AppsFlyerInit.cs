using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppsFlyerInit : MonoBehaviour
{
    private void Awake()
    {
        // Set AppsFlyer’s Developer key.
        const string APPSFLYER_DEV_KEY = "zcKrZYJWnrWWctCxcLNnyT";
        AppsFlyer.setAppsFlyerKey(APPSFLYER_DEV_KEY);
        // For detailed logging
        // AppsFlyer.setIsDebug (true);
        // Set Android package name
        AppsFlyer.setAppID(Application.identifier);
        AppsFlyer.init(APPSFLYER_DEV_KEY);
    }
}
