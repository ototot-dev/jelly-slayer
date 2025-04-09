using Game;
using UnityEngine;

public class UIGameOver : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnClick() 
    {
        gameObject.SetActive(false);
        GameManager.Instance.CloseGame();

        // GameObject.Find("Launcher").GetComponent<Launcher>().SetMode(Launcher.GameModes.Title);

    }
}
