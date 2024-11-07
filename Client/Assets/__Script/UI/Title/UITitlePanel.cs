using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UITitlePanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void OnClickStart()
    {
        SceneManager.LoadScene("TestScene");
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
