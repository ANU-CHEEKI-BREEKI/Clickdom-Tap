using UnityEngine;
using System.Collections;
using System;

public class SettingsHolder : ASavable
{
    public class SettingsChangedEventArgs : EventArgs
    {
        public Settings OldSettings { get; }
        public Settings NewSettings { get; }

        public SettingsChangedEventArgs(Settings oldSettings, Settings newSettings)
        {
            OldSettings = oldSettings;
            NewSettings = newSettings;
        }
    }

    public delegate void SettingsChangedEventHandler(SettingsHolder holder, SettingsChangedEventArgs args);
    public event SettingsChangedEventHandler OnSettingsShanged;

    [SerializeField] private Settings settings;
    public Settings Settings
    {
        get => settings;
        set
        {
            if (settings != value)
            {
                var old = settings;
                settings = value;
                OnSettingsShanged?.Invoke(this, new SettingsChangedEventArgs(old, settings));
            }
        }
    }

    private Settings recordedSettings = new Settings();

    public void UndoSettings()
    {
        Settings = recordedSettings;
    }

    public void RecordSettings()
    {
        recordedSettings = JsonUtility.FromJson<Settings>(
            JsonUtility.ToJson(settings)
        );
    }


}
