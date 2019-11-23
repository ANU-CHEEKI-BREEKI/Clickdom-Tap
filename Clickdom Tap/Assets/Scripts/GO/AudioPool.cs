using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPool : MonoBehaviour
{
    public enum InitTime { AWAKE, START }

    [SerializeField] private AudioSource template;
    [SerializeField] private int poolCapacity = 60;
    [SerializeField] private int initPoolCount = 20;
    [SerializeField] private InitTime initTime = InitTime.START;
    
    private int poolCount;
    private Queue<AudioSource> sources = new Queue<AudioSource>();
    private List<AudioSource> usedSources = new List<AudioSource>();

    private bool inited = false;

    private void Awake()
    {
        if (initTime == InitTime.AWAKE)
            Init();
    }

    private void Start()
    {
        if (initTime == InitTime.START)
            Init();
    }

    private void Update()
    {
        var cnt = usedSources.Count;
        var releasedCount = 0;
        for (int i = 0; i < cnt - releasedCount; i++)
        {
            var source = usedSources[i];
            if (!source.isPlaying)
            {
                var last = cnt - releasedCount - 1;
                usedSources[i] = usedSources[last];
                usedSources[last] = null;
                ReleaceSource(source);
                releasedCount++;
                i--;
            }
        }

        if(releasedCount > 0 && cnt > 0)
            usedSources.RemoveRange(cnt - releasedCount, releasedCount);
    }

    public void Init()
    {
        if (inited)
            return;

        inited = true;

        var cache = new Queue<AudioSource>(initPoolCount);
        while(poolCount < poolCapacity)
            cache.Enqueue(GetSource());
        while (cache.Count > 0)
            ReleaceSource(cache.Dequeue());
    }

    public AudioSource GetSource()
    {
        AudioSource source = null;
        if (sources.Any())
        {
            source =  sources.Dequeue();
        }
        else if (poolCount < poolCapacity)
        {
            poolCount++;
            if (template != null)
            {
                source =  Instantiate(template, transform, false).GetComponent<AudioSource>();
            }
            else
            {
                var go = new GameObject();
                go.transform.SetParent(transform);
                source = go.AddComponent<AudioSource>();
            }
        }

        if (source != null)
        {
            source.enabled = true;
            usedSources.Add(source);
        }

        return source;
    }

    public void ReleaceSource(AudioSource source)
    {
        usedSources.Remove(source);

        source.enabled = false;
        sources.Enqueue(source);

        var count = sources.Count;

        if (count > poolCapacity)
            poolCapacity = count;

        if (count > poolCount)
            poolCount = count;
    }

}
