using UnityEngine;
using UnityEngine.SceneManagement;

public class UIGameMenu : MonoBehaviour
{
    [SerializeField]
    Animator _anim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnClickResume() 
    {
        gameObject.SetActive(false);
    }
    public void OnClickGameExit() 
    {
        SceneManager.LoadScene("Title");
    }
}
