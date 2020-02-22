using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerPrefsSaveLoader : ISaveLoader
{
    public void Load(string id, Action<bool, string> onLoaded = null)
    {
        var data = PlayerPrefs.GetString(id);

        var bytes = Encoding.ASCII.GetByteCount(data);
        FirebaseAnalyticsWrapper.LogLocalLoadedDataEvent(bytes, nameof(PlayerPrefs));

        onLoaded?.Invoke(true, data);
    }
    
    public void Save(string id, string data, Action<bool> onSaved = null)
    {
        PlayerPrefs.SetString(id, data);
        PlayerPrefs.Save();

        var bytes = Encoding.ASCII.GetByteCount(data);
        FirebaseAnalyticsWrapper.LogLocalSavedDataEvent(bytes, nameof(PlayerPrefs));

        onSaved?.Invoke(true);
    }
}
