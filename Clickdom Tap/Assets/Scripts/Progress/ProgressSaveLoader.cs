using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ProgressSaveLoader : MonoBehaviour
{
    private ASavable[] progreses;
    private ISaveLoader saveloader;
    private const string progressSavedKey = "user progress";

    public void InitSaveLoader()
    {
        saveloader = new PlayerPrefsSaveLoader();
    }

    [ContextMenu("Awake")]
    private void Awake()
    {
        InitSaveLoader();
        Load();
    }

    private void OnLevelWasLoaded(int level)
    {

    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
            Save();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    [ContextMenu("ResetSavedData")]
    public void ResetSavedData()
    {
        InitSaveLoader();
        saveloader.Save(progressSavedKey, "");
    }

    [Serializable]
    private class SavedData
    {
        public int id;
        public string data;
    }
    [Serializable]
    private class SavedCollection
    {
        public List<SavedData> collection = new List<SavedData>();
    } 

    private void Save()
    {
        Debug.Log("ProgressSaveLoader - Save");

        progreses = FindObjectsOfType<ASavable>();

        var saved = new SavedCollection();

        foreach (var item in progreses)
            saved.collection.Add(new SavedData() { id = item.Id, data = JsonUtility.ToJson(item) });

        var toSaveData = JsonUtility.ToJson(saved);
        saveloader.Save(progressSavedKey, toSaveData);
        Debug.Log(toSaveData);
    }

    private void Load()
    {
        Debug.Log("ProgressSaveLoader - Load");

        var data = saveloader.Load(progressSavedKey);
        var saved = JsonUtility.FromJson<SavedCollection>(data);

        if (saved == null || saved.collection == null)
            return;

        progreses = FindObjectsOfType<ASavable>();
        foreach (var item in progreses)
        {
            var id = item.Id;
            var loadedData = saved.collection.Where(s => s.id == id).FirstOrDefault();
            if(loadedData != null)
                JsonUtility.FromJsonOverwrite(
                    loadedData.data,
                    item
                );
        }
    }


}
