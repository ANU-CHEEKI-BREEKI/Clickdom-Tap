using ANU.GoogleWrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RewardedAdInitializator : MonoBehaviour
{
    private void Awake()
    {
        var allAdapters = FindObjectsOfType<RewardedAdAdapter>();

        //to be shure what is initialized
        var mtd = MainThreadDispatcher.Instance;

        GAds.Instance.Init(
            debugMode: false, 
            ()=>
            {
                mtd.Enqueue(() =>
                {
                    foreach (var adapter in allAdapters)
                        adapter.Init();
                });
            },
            false
        );
    }
}
