using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game;

public class UITitlePanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        InitManager.Initialize();
    }
    public void OnClickGameStart()
    {
        SceneManager.LoadScene("BattleTest");
    }
    public void OnClickTutorial()
    {
        SceneManager.LoadScene("BattleTest");
    }

    public void OnClickGameExit()
    {
        Application.Quit();
    }
}
