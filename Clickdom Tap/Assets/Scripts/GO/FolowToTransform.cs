using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolowToTransform : MonoBehaviour
{
    [SerializeField] private Transform toFolow;
    [SerializeField] private bool onlyOnStart = true;
    [SerializeField] [Range(0f, 1f)] private float updateDeltaTime = 0.2f;
    [SerializeField] private bool scaledTime = true;

    private Coroutine updateCoroutime;
    private Transform _transfrom;

    private void Start()
    {
        _transfrom = transform;

        Folow();

        if (!onlyOnStart)
            updateCoroutime = StartCoroutine(FolowUpdate());
    }

    private void OnDestroy()
    {
        if (updateCoroutime != null)
            StopCoroutine(updateCoroutime);
    }

    private void Folow()
    {
        if(_transfrom != null)
            _transfrom.position = toFolow.position;
    }

    private IEnumerator FolowUpdate()
    {
        while (true)
        {
            if (scaledTime)
                yield return new WaitForSeconds(updateDeltaTime);
            else
                yield return new WaitForSecondsRealtime(updateDeltaTime);

            Folow();
        } 
    }
}
