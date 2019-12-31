using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitScript : MonoBehaviour
{
    public void QuitGame()
    {
        print("==================== quit game...");
        Application.Quit();
    }
}
