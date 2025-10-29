using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void StartGame() {
        SceneManager.LoadScene("Scenes/MainScene");
    }
    
    public void OpenSettings() {}

    public void ExitGame() {
        Application.Quit();
    }
}
