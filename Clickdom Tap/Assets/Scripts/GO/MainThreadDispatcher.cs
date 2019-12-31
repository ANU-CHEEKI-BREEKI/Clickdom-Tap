using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    private static MainThreadDispatcher _instance = null;
    public static MainThreadDispatcher Instance
    {
        get
        {
            if(_instance == null)
                CreateNewInstance(null);
            return _instance;
        }
    }

    private static void CreateNewInstance(MainThreadDispatcher component = null)
    {
        if (component == null)
            new GameObject().AddComponent<MainThreadDispatcher>();
        else
            _instance = component;

        DontDestroyOnLoad(_instance.gameObject);
    }

    private void Awake()
    {
        if (_instance == null)
            CreateNewInstance(this);
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
                _executionQueue
                    .Dequeue()
                    .Invoke();
        }
    }

    /// <summary>
    /// Locks the queue and adds the IEnumerator to the queue
    /// </summary>
    /// <param name="action">IEnumerator function that will be executed as coroutine from the main thread.</param>
    public void Enqueue(IEnumerator action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() => StartCoroutine(action));
        }
    }

    /// <summary>
    /// Locks the queue and adds the Action to the queue
    /// </summary>
    /// <param name="action">function that will be executed from the main thread.</param>
    public void Enqueue(Action action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}
