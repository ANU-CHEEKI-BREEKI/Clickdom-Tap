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

    public GeneralSettings General => general;
    public AudioSettings Audio => audio;
    public GraphixSettings Grapgix => grapgix;
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
    [SerializeField] private bool displaySoldiersShadows = true;
    [SerializeField] private bool displayArrowsShadows = true;
    [SerializeField] private bool displayPopUpNumbers = true;
    [SerializeField] private bool displayHelperUi = true;

    public event Action<bool> OnDisplaySoldiersShadowsChanged;
    public event Action<bool> OnDisplayArrowsShadowsChanged;
    public event Action<bool> OnDisplayPopUpNumbersChanged;
    public event Action<bool> OnDdisplayHelperUiChanged;

    public bool DisplaySoldiersShadows
    {
        get => displaySoldiersShadows;
        set
        {
            if (displaySoldiersShadows != value)
            {
                displaySoldiersShadows = value;
                OnDisplaySoldiersShadowsChanged?.Invoke(displaySoldiersShadows);
            }
        }
    }

    public bool DisplayArrowsShadows
    {
        get => displayArrowsShadows;
        set
        {
            if (displayArrowsShadows != value)
            {
                displayArrowsShadows = value;
                OnDisplayArrowsShadowsChanged?.Invoke(displayArrowsShadows);
            }
        }
    }

    public bool DisplayPopUpNumbers
    {
        get => displayPopUpNumbers;
        set
        {
            if (displayPopUpNumbers != value)
            {
                displayPopUpNumbers = value;
                OnDisplayPopUpNumbersChanged?.Invoke(displayPopUpNumbers);
            }
        }
    }

    public bool DisplayHelperUi
    {
        get => displayHelperUi;
        set
        {
            if (displayHelperUi != value)
            {
                displayHelperUi = value;
                OnDdisplayHelperUiChanged?.Invoke(displayHelperUi);
            }
        }
    }
}