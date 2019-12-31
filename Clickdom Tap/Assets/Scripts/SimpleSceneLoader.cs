using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SimpleSceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private bool loadOnAwake;
    [SerializeField] private bool loadAsync;
    [Space]
    [SerializeField] private UnityEvent OnSceneLoaded;

    private void Awake()
    {
        if (loadOnAwake)
            LoadScene();
    }

    private void Start()
    {
        if (!loadOnAwake)
            LoadScene();
    }

    private void LoadScene()
    {
        if (loadAsync)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = true;
            operation.completed += (op) =>
            {
                OnSceneLoaded?.Invoke();
            };
        }
        else
        {
            SceneManager.LoadScene(sceneName);
            OnSceneLoaded?.Invoke();
        }
    }
}
