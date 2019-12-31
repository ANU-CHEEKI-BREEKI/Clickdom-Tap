using ANU.GoogleWrap;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ProgressSaveLoader : MonoBehaviour
{
    [Serializable] public class StringUnityEvent : UnityEvent<string> { }

    [SerializeField] private UnityEvent onDataLoaded;
    [SerializeField] private bool applyLoadedDataOnAwake = false;
    [Space]
    [SerializeField] private UnityEvent onDataSaved;
    [SerializeField] private UnityEvent onDataSavedForQuit;
    [Space]
    [SerializeField] private StringUnityEvent onStartOperation;
    [SerializeField] private StringUnityEvent onEndOperation;

    public static ProgressSaveLoader Instance { get; set; }

    private bool isDataLoaded = false;
    private bool IsDataLoaded
    {
        get => isDataLoaded;
        set
        {
            isDataLoaded = value;
            if (value)
                onDataLoaded?.Invoke();
        }
    }
    private string loadedData = null;

    private ASavable[] progreses;
    private ISaveLoader saveloader;
    /// <summary>
    /// ОСТОРОЖНО. ВИДИМО, ДЛЯ ГУГЛА НЕЛЬЗЯ ИСПОЛЬЗОВАТЬ ПРОБЕЛЫ...
    /// </summary>
    private const string progressSavedKey = "user_progress";

    private PlayerPrefsSaveLoader prefsSaveLoader;
    private GPSSavedGames googleSaveLoader;

    private const string LOAD_START = "loading saved data...";
    private const string SAVE_START = "saving data...";

    public void InitSaveLoader(Action onInit)
    {
        prefsSaveLoader = new PlayerPrefsSaveLoader();
        googleSaveLoader = new GPSSavedGames((success) =>
        {
            if (!success)
                saveloader = prefsSaveLoader;
            else
                saveloader = googleSaveLoader;

            Debug.Log("GPSSavedGames authenticate is: " + success);
            Debug.Log("saveloader is: " + saveloader.GetType());

            onInit?.Invoke();
        });
    }

    [ContextMenu("Awake")]
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (!Instance.isDataLoaded)
            {
                Instance.onStartOperation?.Invoke(LOAD_START);
                Instance.InitSaveLoader(() =>
                {
                    Instance.Load((success, data) =>
                    {
                        //prefsSaeLoaderv работает синхронно, так что можно сделать так
                        if (Instance.saveloader is GPSSavedGames && (!success || string.IsNullOrEmpty(data)))
                            prefsSaveLoader.Load(progressSavedKey, (s, d)=> { data = d; });

                        Instance.loadedData = data;

                        Instance.IsDataLoaded = true;

                        if (Instance.applyLoadedDataOnAwake)
                            ApplyLoadedData(success, loadedData);
                    });
                    onEndOperation.Invoke(LOAD_START);
                });
            }
        }
        else
        {
            if (applyLoadedDataOnAwake && Instance.IsDataLoaded)
                Instance.ApplyLoadedData(true, Instance.loadedData);
        }
    }

    private void ApplyLoadedData(bool success, string data)
    {
        if (success)
        {
            var saved = JsonUtility.FromJson<SavedCollection>(data);

            if (saved == null || saved.collection == null)
                return;

            progreses = FindObjectsOfType<ASavable>();
            foreach (var item in progreses)
            {
                var id = item.Id;
                var loadedData = saved.collection.Where(s => s.id == id).FirstOrDefault();
                if (loadedData != null)
                    JsonUtility.FromJsonOverwrite(
                        loadedData.data,
                        item
                    );
            }

            Debug.Log("loaded");
        }
        else
        {
            Debug.Log("load error");
        }

        onEndOperation.Invoke(LOAD_START);
    }

    private void OnApplicationPause(bool pause)
    {
        if (Instance != this)
            return;

        if(pause)
            Save();
    }

    private void OnApplicationQuit()
    {
        if (Instance != this)
            return;

        SaveForQuit();
    }

    [ContextMenu("ResetSavedData")]
    public void ResetSavedData()
    {
        onStartOperation?.Invoke(SAVE_START);
        InitSaveLoader(()=>
        {
            saveloader.Save(progressSavedKey, "");
            onEndOperation.Invoke(SAVE_START);
        });
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

    private bool saveForQuitCall = false;
    public void SaveForQuit()
    {
        if (Instance != this)
        {
            Instance.SaveForQuit();
            return;
        }

        saveForQuitCall = true;

        if (saveloader != prefsSaveLoader)
            SaveWithAction(()=>
            {
                //дублируем сохранения в локально, чтобы можно было продолжить без интернета
                saveloader = prefsSaveLoader;
                SaveWithAction(()=> { onDataSavedForQuit?.Invoke(); });
            });
         else
            SaveWithAction(() => { onDataSavedForQuit?.Invoke(); });
    }

    public void Save()
    {
        if (Instance != this)
        {
            Instance.Save();
            return;
        }

        SaveWithAction(null);
    }

    private void SaveWithAction(Action onSaved)
    {
        onStartOperation?.Invoke(SAVE_START);

        Debug.Log("ProgressSaveLoader - Save");

        progreses = FindObjectsOfType<ASavable>();

        var saved = new SavedCollection();

        foreach (var item in progreses)
            saved.collection.Add(new SavedData() { id = item.Id, data = JsonUtility.ToJson(item) });

        var toSaveData = JsonUtility.ToJson(saved);
        saveloader.Save(
            progressSavedKey,
            toSaveData,
            (success) =>
            {
                if (success)
                    Debug.Log("saved");
                else
                    Debug.Log("save error");
                onEndOperation.Invoke(SAVE_START);

                onSaved?.Invoke();
                onDataSaved?.Invoke();
            }
        );
        Debug.Log(toSaveData);
    }

    private void Load(Action<bool, string> onLoaded)
    {
        onStartOperation?.Invoke(LOAD_START);

        Debug.Log("ProgressSaveLoader - Load");
        saveloader.Load(progressSavedKey, onLoaded);
    }
}
