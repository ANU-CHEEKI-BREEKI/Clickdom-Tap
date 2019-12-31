using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; set; }

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private float oldTimeScale = 1;
    public void FullPauseGame()
    {
        oldTimeScale = Time.timeScale;
        Time.timeScale = 0;
    }

    public void ResumeGame()
    {
        Time.timeScale = oldTimeScale;
    }
}
