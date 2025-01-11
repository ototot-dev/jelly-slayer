using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game;

public class UITitlePanel : MonoBehaviour
{
    int _cursorIndex = 0;
    public Transform _trCursor;
    public Transform[] _trButton;

    // Start is called before the first frame update
    void Start()
    {
        InitManager.Initialize();
    }
    public void OnClickGameStart()
    {
        SetCursorIndex(0);
        Invoke("DoAction", 0.1f);
    }
    public void OnClickTutorial()
    {
        SetCursorIndex(1);
        Invoke("DoAction", 0.1f);
    }
    public void OnClickGameExit()
    {
        SetCursorIndex(2);
        Invoke("DoAction", 0.1f);
    }
    void SetCursorIndex(int index) 
    {
        if(index < 0)
            index = 0;
        if (index >= 3)
            index = 2;

        _cursorIndex = index;
        _trCursor.position = _trButton[index].position;
    }

    void DoAction() 
    {
        switch (_cursorIndex)
        {
            case 0: SceneManager.LoadScene("BattleTest"); break;
            case 1: SceneManager.LoadScene("BattleTest"); break;
            case 2: Application.Quit(); break;
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetCursorIndex(_cursorIndex - 1);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetCursorIndex(_cursorIndex + 1);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            DoAction();
        }
    }
}
