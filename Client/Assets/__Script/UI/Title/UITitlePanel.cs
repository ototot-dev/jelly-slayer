using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game;

public class UITitlePanel : MonoBehaviour
{
    int _cursorIndex = 0;

    public RectTransform _rtLogo;
    public RectTransform _rtMenu;

    public Transform _trCursor;
    public UITItleButton[] _buttons;

    public SocialAvatar _avatar;

    // Start is called before the first frame update
    void Start()
    {
        InitManager.Initialize();

        float ratio = (float)Screen.width / (float)Screen.height;
        ratio /= (1280f / 800f);
        ratio--;
        if(ratio < 0)
            ratio = 0;

        _rtLogo.anchoredPosition += new Vector2(0, 150 * ratio);
        _rtMenu.anchoredPosition -= new Vector2(0, 150 * ratio);

        SetCursorIndex(0);

        _avatar.SetEmotion(EMOTION.Angry);
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

        SelectButton(index);
    }
    void SelectButton(int index) 
    {
        _cursorIndex = index;
        _trCursor.position = _buttons[index].transform.position;

        for (int ia = 0; ia < _buttons.Length; ia++) 
        {
            _buttons[ia].Select(ia == index);
        }
    }

    void DoAction() 
    {
        switch (_cursorIndex)
        {
            case 0: SceneManager.LoadScene("BattleTest"); break;
            case 1: SceneManager.LoadScene("Tutorial"); break;
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
