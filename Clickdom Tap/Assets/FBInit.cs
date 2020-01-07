using Facebook.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FBInit : MonoBehaviour
{
    private void Awake()
    {
        if (!FB.IsInitialized)
        {
            FB.Init(
                onInitComplete: () =>
                {
                    if (FB.IsInitialized)
                    {
                        FB.ActivateApp();
                        print("FB app Activated");
                    }
                    else
                    {
                        print("FB app init failed");
                    }
                },
                onHideUnity: (isGameShown) =>
                {
                    if (!isGameShown)
                        PauseManager.Instance?.FullPauseGame();
                    else
                        PauseManager.Instance?.ResumeGame();
                }
            );
        }
        else
        {
            FB.ActivateApp();
            print("FB app Activated");
        }
    }
}
