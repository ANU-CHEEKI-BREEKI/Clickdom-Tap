using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AudioSettingsVolumeSliderBinding : MonoBehaviour
{
    public enum VolumeType { MASTER, MUSIC, EFFECTS, UI }

    [SerializeField] private SettingsHolder settings;
    [SerializeField] private VolumeType type;

    [SerializeField] private bool initSliderOnStart = true;

    private Slider slider;

    private Action<float> onSliderValChanged;
    private Action<Settings> onSettingsChanged;
    private bool callEventFromSettings = false;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        switch (type)
        {
            case VolumeType.MASTER:
                onSliderValChanged = (val) =>
                {
                    settings.Settings.Audio.MasterVolume = val;
                };
                onSettingsChanged = (set) =>
                {
                    slider.value = set.Audio.MasterVolume;
                };
                break;
            case VolumeType.MUSIC:
                onSliderValChanged = (val) =>
                {
                    settings.Settings.Audio.MusicVolume = val;
                };
                onSettingsChanged = (set) =>
                {
                    slider.value = set.Audio.MusicVolume;
                };
                break;
            case VolumeType.EFFECTS:
                onSliderValChanged = (val) =>
                {
                    settings.Settings.Audio.EffectsVolume = val;
                };
                onSettingsChanged = (set) =>
                {
                    slider.value = set.Audio.EffectsVolume;
                };
                break;
            case VolumeType.UI:
                onSliderValChanged = (val) =>
                {
                    settings.Settings.Audio.UiVolume = val;
                };
                onSettingsChanged = (set) =>
                {
                    slider.value = set.Audio.UiVolume;
                };
                break;
        }
        slider.onValueChanged.AddListener(MethodWrap);

        settings.OnSettingsShanged += Settings_OnSettingsShanged;

        if (initSliderOnStart)
            Settings_OnSettingsShanged(settings, null);
    }

    private void OnDestroy()
    {
        slider.onValueChanged.RemoveListener(MethodWrap);

        if(settings != null)
            settings.OnSettingsShanged -= Settings_OnSettingsShanged;
    }

    private void MethodWrap(float val)
    {
        if (callEventFromSettings)
            return;

        onSliderValChanged.Invoke(val);
    }

    private void Settings_OnSettingsShanged(SettingsHolder holder, SettingsHolder.SettingsChangedEventArgs args)
    {
        callEventFromSettings = true;

        onSettingsChanged.Invoke(holder.Settings);

        callEventFromSettings = false;
    }
}
