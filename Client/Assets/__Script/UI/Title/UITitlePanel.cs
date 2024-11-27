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
    public void OnClickStart()
    {
        SceneManager.LoadScene("BattleTest");
    }
    public void OnClickGameExit()
    {
        Application.Quit();
    }    
    // Update is called once per frame
    void Update()
    {
    }
}
