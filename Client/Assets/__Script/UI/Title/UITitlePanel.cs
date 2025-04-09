using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Game;
using DG.Tweening;

public class UITitlePanel : MonoBehaviour
{
    int _cursorIndex = 0;

    public RectTransform _rtLogo;
    public RectTransform _rtMenu;
    public Image _imageShadow;
    public RawImage _imageRender;

    public Transform _trCursor;
    public UITItleButton[] _buttons;

    public SocialAvatar _avatar;
    public GameObject _cutSceneObj;
    public Volume _volume;

    public float _duration = 1f; // �ִϸ��̼� ���� �ð�
    private bool _isGlitch = false;
    private float _value = 0f;

    public AudioClip[] _clipList;

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

        SoundManager.Instance.PlayBGM(SoundID.BGM_TITLE);
    }
    private void OnEnable()
    {
        var logopos = _rtLogo.anchoredPosition;
        var menupos = _rtMenu.anchoredPosition;
        _rtLogo.anchoredPosition = new Vector3(logopos.x - 3000, logopos.y);
        _rtMenu.anchoredPosition = new Vector3(menupos.x, menupos.y - 600);

        _rtLogo.DOLocalMoveX(0, 0.4f).SetDelay(0.2f);
        _rtMenu.DOLocalMoveY(-200, 0.3f).SetDelay(3.2f);

        _cutSceneObj.SetActive(true);
        _imageShadow.color = new Color(0, 0, 0, 1);
        _imageRender.color = new Color(0, 0, 0, 1);

        _volume.enabled = false;
        _imageRender.DOColor(new Color(1, 1, 1, 1), 1.2f).SetDelay(1.5f)
            .OnStart(() => {
                _volume.enabled = true;
                DoGlitch();
            })
            .OnComplete(() => {

            });
    }
    public void OnClickButton(int index)
    {
        SetCursorIndex(index);
        Invoke("DoAction", 0.1f);
    }
    public void OnHoverButton(int index)
    {
        SetCursorIndex(index);
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
        if (_cursorIndex != index)
        {
            _cursorIndex = index;
            SoundManager.Instance.PlayWithClip(_clipList[0]);
        }
        _trCursor.position = _buttons[index].transform.position;

        for (int ia = 0; ia < _buttons.Length; ia++) 
        {
            _buttons[ia].Select(ia == index);
        }
    }

    void DoAction() 
    {
        SoundManager.Instance.PlayWithClip(_clipList[1]);
        switch (_cursorIndex)
        {
            case 0:
                _cutSceneObj.SetActive(false);
                // GameObject.Find("Launcher").GetComponent<Launcher>().SetMode(Launcher.GameModes.Game);
                DoCloseAction();
                break;
            case 1:
                _cutSceneObj.SetActive(false);
                // GameObject.Find("Launcher").GetComponent<Launcher>().SetMode(Launcher.GameModes.Tutorial);
                DoCloseAction();
                break;
            case 2: Application.Quit(); break;
        }
    }
    void DoCloseAction() 
    {
        _volume.enabled = false;

        var logopos = _rtLogo.anchoredPosition;
        var menupos = _rtMenu.anchoredPosition;
        _rtLogo.DOMoveX(3000, 0.7f).SetDelay(0.4f);
        _rtMenu.DOMoveY(-400, 0.7f).SetDelay(0.4f);

        _imageShadow.DOColor(new Color(0, 0, 0, 0), 0.8f).SetDelay(1.5f);
        _imageRender.DOColor(new Color(0, 0, 0, 0), 0.8f).SetDelay(0.1f);

        DOVirtual.DelayedCall(2.4f, () => {
            gameObject.SetActive(false);
            _rtLogo.anchoredPosition = logopos;
            _rtMenu.anchoredPosition = menupos;
            _imageShadow.color = new Color(0, 0, 0, 1);
        });

    }
    void DoGlitch() 
    {
        _isGlitch = true;

        DOTween.To(() => _value, x => _value = x, 1f, _duration)
            .OnUpdate(() => { _volume.weight = _value; })
            .OnComplete(() =>
            {
                DOTween.To(() => _value, x => _value = x, 0f, _duration)
                .OnUpdate(() => { _volume.weight = _value; })
                .OnComplete(() => { _isGlitch = false; });
            });
        DOVirtual.DelayedCall(0.1f, () => {
            int rand = Random.Range(2, 5);
            SoundManager.Instance.PlayWithClip(_clipList[rand]);
        });
    }
    private void Update()
    {
        if (_isGlitch == false)
        {
            var rand = Random.Range(0, 1000);
            if (rand == 0)
            {
                DoGlitch();
            }
        }
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
