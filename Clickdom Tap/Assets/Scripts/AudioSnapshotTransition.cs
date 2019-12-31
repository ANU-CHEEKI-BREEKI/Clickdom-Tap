using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSnapshotTransition : MonoBehaviour
{
    [SerializeField] private TransitionSettings[] snapshots;

    public void MakeTransitions()
    {
        StartCoroutine(Transitions());
    }

    private IEnumerator Transitions()
    {
        foreach (var sett in snapshots)
        {
            sett.snapshot.TransitionTo(sett.timeToReachThisSnapshot);
            yield return new WaitForSeconds(sett.timeToReachThisSnapshot);
        }
    }

    [System.Serializable]
    public class TransitionSettings
    {
        public AudioMixerSnapshot snapshot;
        public float timeToReachThisSnapshot;
    }
}
