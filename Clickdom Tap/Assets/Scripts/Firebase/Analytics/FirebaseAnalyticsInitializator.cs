using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirebaseAnalyticsInitializator : MonoBehaviour
{
    private void Start()
    {
        FirebaseAnalyticsWrapper.Init();
    }
}
