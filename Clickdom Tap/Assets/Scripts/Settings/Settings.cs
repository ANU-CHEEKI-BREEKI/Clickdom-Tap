using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Settings
{
    [SerializeField] private GeneralSettings general = new GeneralSettings();
    [SerializeField] private AudioSettings audio = new AudioSettings();
    [SerializeField] private GraphixSettings grapgix = new GraphixSettings();

    public AudioSettings Audio => audio;
}

[Serializable]
public class GeneralSettings
{
  
}

[Serializable]
public class AudioSettings 
{
    [SerializeField] private float masterVolume = 0.5f;
    [SerializeField] private float musicVolume = 1;
    [SerializeField] private float effectsVolume = 1;
    [SerializeField] private float uiVolume = 1;

    public event Action<float> OnMasterVolumeChanged;
    public event Action<float> OnMusicVolumeChanged;
    public event Action<float> OnEffectsVolumeChanged;
    public event Action<float> OnUIVolumeChanged;

    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            if (masterVolume != value)
            {
                masterVolume = value;
                OnMasterVolumeChanged?.Invoke(masterVolume);
            }
        }
    }
    public float MusicVolume
    {
        get=> musicVolume;
        set
        {
            if (musicVolume != value)
            {
                musicVolume = value;
                OnMusicVolumeChanged?.Invoke(musicVolume);
            }
        }
    }
    public float EffectsVolume
    {
        get=>effectsVolume;
        set
        {
            if (effectsVolume != value)
            {
                effectsVolume = value;
                OnEffectsVolumeChanged?.Invoke(effectsVolume);
            }
        }
    }
    public float UiVolume
    {
        get => uiVolume;
        set
        {
            if (uiVolume != value)
            {
                uiVolume = value;
                OnUIVolumeChanged?.Invoke(uiVolume);
            }
        }
    }
}

[Serializable]
public class GraphixSettings
{
    [SerializeField] private bool soldiersShadows = true;
    [SerializeField] private bool arrowsShadows = true;
    [SerializeField] private bool popUpNumbers = true;
}