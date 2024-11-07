using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIGameClose : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void OnClickClose()
    {
        SceneManager.LoadScene("Title");
    }
}
