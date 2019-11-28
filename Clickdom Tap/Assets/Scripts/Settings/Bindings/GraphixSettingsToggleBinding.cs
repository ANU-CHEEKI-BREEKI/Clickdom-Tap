using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Toggle))]
public class GraphixSettingsToggleBinding : MonoBehaviour
{
    public enum ToggleType { SOLDIER_SHADOWS, ARROWS_SHADOWS, POP_UP_NUMBERS, HELPER_UI }

    [SerializeField] private SettingsHolder settings;
    [SerializeField] private ToggleType type;

    [SerializeField] private bool initSliderOnStart = true;

    private Slider slider;

    private Action<bool> onToggleValChanged;
    private Action<Settings> onSettingsChanged;
    private bool callEventFromSettings = false;

    private Toggle toggle;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    private void Start()
    {
        switch (type)
        {
            case ToggleType.SOLDIER_SHADOWS:
                onToggleValChanged = (val) =>
                {
                    settings.Settings.Grapgix.DisplaySoldiersShadows = val;
                };
                onSettingsChanged = (set) =>
                {
                    toggle.isOn = set.Grapgix.DisplaySoldiersShadows;
                };
                break;
            case ToggleType.ARROWS_SHADOWS:
                onToggleValChanged = (val) =>
                {
                    settings.Settings.Grapgix.DisplayArrowsShadows = val;
                };
                onSettingsChanged = (set) =>
                {
                    toggle.isOn = set.Grapgix.DisplayArrowsShadows;
                };
                break;
            case ToggleType.POP_UP_NUMBERS:
                onToggleValChanged = (val) =>
                {
                    settings.Settings.Grapgix.DisplayPopUpNumbers = val;
                };
                onSettingsChanged = (set) =>
                {
                    toggle.isOn = set.Grapgix.DisplayPopUpNumbers;
                };
                break;
            case ToggleType.HELPER_UI:
                onToggleValChanged = (val) =>
                {
                    settings.Settings.Grapgix.DisplayHelperUi = val;
                };
                onSettingsChanged = (set) =>
                {
                    toggle.isOn = set.Grapgix.DisplayHelperUi;
                };
                break;
        }

        toggle.onValueChanged.AddListener(MethodWrap);

        settings.OnSettingsShanged += Settings_OnSettingsShanged;

        if (initSliderOnStart)
            Settings_OnSettingsShanged(settings, null);
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(MethodWrap);

        if (settings != null)
            settings.OnSettingsShanged -= Settings_OnSettingsShanged;
    }

    private void MethodWrap(bool val)
    {
        if (callEventFromSettings)
            return;

        onToggleValChanged.Invoke(val);
    }

    private void Settings_OnSettingsShanged(SettingsHolder holder, SettingsHolder.SettingsChangedEventArgs args)
    {
        callEventFromSettings = true;

        onSettingsChanged.Invoke(holder.Settings);

        callEventFromSettings = false;
    }
}
