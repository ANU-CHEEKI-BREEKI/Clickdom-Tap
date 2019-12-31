
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(AudioSource))]
public class AudioPlayer : MonoBehaviour
{
    [Header("Pool of audio sources. If null, player will try to get audio source as this GO component")]
    [SerializeField] private AudioPool pool;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] [Range(0, 0.5f)] private float pitchRandRange = 0.01f;
    [SerializeField] private ScaleByPositionSettings volumeScale;
    [Space]
    [SerializeField] private AudioSettings[] clips;
    
    private AudioSource source;

    private void Awake()
    {
        if (pool == null)
            source = GetComponent<AudioSource>();

        if (!playOnAwake)
            return;

        source = InitAudioSource(null, transform.position);

        if (source != null)
            source.Play();
        else
            throw new System.Exception($"You must set reference to audio pool, or add AudioSource component to this GameObject");
    }

    public void PlayRandomClip(Vector3 position)
    {
        PlayClipAtPos(Random.Range(0, clips.Length), position);
    }

    public void PlayRandomClip()
    {
        PlayClipAtPos(Random.Range(0, clips.Length), transform.position);
    }

    public void PlayClipAtPos(int index, Vector3 position)
    {
        var s = clips[index];

        source = InitAudioSource(s, position);

        if (source != null)
            source.Play();
    }

    public void PlayClip(int index)
    {
        PlayClipAtPos(index, transform.position);
    }
    
    public void StopPlaying()
    {
        if (source != null)
        {
            source.Stop();
            //if (pool != null)
            //    pool.ReleaceSource(source);
        }
    }

    private AudioSource InitAudioSource(AudioSettings settings, Vector3 position)
    {
        if (pool != null)
        {
            pool.Init();
            source = pool.GetSource();
        }

        if (source != null)
        {
            var pitchRange = pitchRandRange;
            if (settings != null)
            {
                source.clip = settings.clip;
                source.loop = settings.loop;
                source.volume = settings.volume;

                if (settings.rewriteStereoPan)
                    source.panStereo = settings.stereoPan;

                if (settings.rewritePriority)
                    source.priority = settings.priority;

                if (settings.rewritePitcRand)
                    pitchRange = settings.pitchRange;
            }

            if (volumeScale != null)
                source.volume *= volumeScale.LerpEvaluete(position);

            source.pitch = 1 + Random.Range(-pitchRange, pitchRange);
        }

        return source;
    }

    [System.Serializable]
    public class AudioSettings
    {
        public AudioClip clip;
        [Space]
        public bool rewritePitcRand = false;
        [Range(0, 0.5f)] public float pitchRange = 0;
        [Space]
        public bool loop = false; 
        [Range(0f, 1f)] public float volume = 1f;
        [Space]
        public bool rewriteStereoPan = false;
        [Range(-1f, 1f)] public float stereoPan = 0;
        [Space]
        public bool rewritePriority = false;
        [Range(0, 256)] public int priority = 128;
    }
}
