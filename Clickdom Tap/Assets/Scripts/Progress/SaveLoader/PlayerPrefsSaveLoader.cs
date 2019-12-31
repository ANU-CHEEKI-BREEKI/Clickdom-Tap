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
        onLoaded?.Invoke(true, data);
    }
    
    public void Save(string id, string data, Action<bool> onSaved = null)
    {
        PlayerPrefs.SetString(id, data);
        PlayerPrefs.Save();
        onSaved?.Invoke(true);
    }
}
