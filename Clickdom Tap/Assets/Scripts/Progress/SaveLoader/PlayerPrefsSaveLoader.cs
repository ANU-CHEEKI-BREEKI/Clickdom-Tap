using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerPrefsSaveLoader : ISaveLoader
{
    public event Action<string> OnSavedAsync;
    public event Action<string> OnLoadedAsync;

    public string Load(string id)
    {
        return PlayerPrefs.GetString(id);
    }

    public Task<string> LoadAsync(string id)
    {
        throw new NotImplementedException();
    }

    public void Save(string id, string data)
    {
        PlayerPrefs.SetString(id, data);
        PlayerPrefs.Save();
    }

    public Task SaveAsync(string id, string data)
    {
        throw new NotImplementedException();
    }
}
