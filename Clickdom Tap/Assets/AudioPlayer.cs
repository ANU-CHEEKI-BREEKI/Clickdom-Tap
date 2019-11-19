using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] [Range(0, 0.5f)] private float pitchRandRange = 0.01f;
    [SerializeField] private ScaleByPositionSettings volumeScale;
    [Space]
    [SerializeField] private AudioSettings[] clips;

    private AudioSource source;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        if (!playOnAwake)
            return;

        InitAudioSource();

        source.Play();
    }

    public void PlayClip(int index)
    {
        var s = clips[index];
        source.clip = s.clip;
        source.loop = s.loop;
        source.volume = s.volume;

        InitAudioSource();

        source.Play();
    }

    public void StopPlaying()
    {
        source.Stop();
    }

    private void InitAudioSource()
    {
        if (volumeScale != null)
            source.volume *= volumeScale.LerpEvaluete(transform.position);

        source.pitch += Random.Range(-pitchRandRange, pitchRandRange);
    }

    [System.Serializable]
    public class AudioSettings
    {
        public AudioClip clip;
        public bool loop; 
        [Range(0f, 1f)] public float volume = 1;
    }
}
