using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game;
using DG.Tweening;

public class UITitlePanel : MonoBehaviour
{
    int _cursorIndex = 0;

    public RectTransform _rtLogo;
    public RectTransform _rtMenu;
    public Image _imageShadow;

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
            //case 0: SceneManager.LoadScene("BattleTest"); break;
            case 0:
                GameObject.Find("Launcher").GetComponent<Launcher>().SetMode(Launcher.GameModes.Game);
                //gameObject.SetActive(false);

                DoCloseAction();
                break;
            //case 1: SceneManager.LoadScene("Tutorial"); break;
            case 1:
                //GameObject.Find("Launcher").GetComponent<Launcher>().SetMode(Launcher.GameModes.Tutorial);
                //gameObject.SetActive(false);

                DoCloseAction();
                break;
            case 2: Application.Quit(); break;
        }
    }
    void DoCloseAction() 
    {
        var logopos = _rtLogo.anchoredPosition;
        var menupos = _rtMenu.anchoredPosition;
        _rtLogo.DOMoveX(2000, 0.7f).SetDelay(0.4f);
        _rtMenu.DOMoveY(-400, 0.7f).SetDelay(0.4f);

        _imageShadow.DOColor(new Color(0, 0, 0, 0), 0.8f).SetDelay(1.5f);

        DOVirtual.DelayedCall(2.4f, () => {
            gameObject.SetActive(false);
            _rtLogo.anchoredPosition = logopos;
            _rtMenu.anchoredPosition = menupos;
            _imageShadow.color = new Color(0, 0, 0, 1);
        });

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
