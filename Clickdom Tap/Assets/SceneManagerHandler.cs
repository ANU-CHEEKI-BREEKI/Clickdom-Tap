using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerHandler : MonoBehaviour
{
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void DestroyAllEcsEntities()
    {
        var manager = World.Active.EntityManager;
        manager.DestroyEntity(manager.UniversalQuery);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
