using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Loader : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SceneManager.LoadScene(0);
    }

    public void LoadDefaultBingoScene() => SceneManager.LoadScene("BingoScreenDefault");
    
    public void LoadSeasonFinalBingoScene() => SceneManager.LoadScene("BingoScreenSeasonFinal");
    
    public void LoadYearFinalBingoScene() => SceneManager.LoadScene("BingoScreenYearFinal");

    public void BackToMenu() => SceneManager.LoadScene("MainMenu");
    
    public void CloseApp() => Application.Quit();
}
