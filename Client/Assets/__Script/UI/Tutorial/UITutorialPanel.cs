using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialPanel : MonoBehaviour
{
    [SerializeField] TutorialManager _manager;

    [SerializeField] GameObject _panelObj;

    [SerializeField] Text _name;
    [SerializeField] Text _text;

    [SerializeField] RawImage[] _portrait;
    [SerializeField] SocialAvatar[] _avatar;

    int _curCursor = 0;
    string _fullText;

    public float _textDelay = 0.03f;
    float _delayRate =  1.0f;

    int _talkIndex = 0;

    Coroutine _coroutine;

    // �ѱ� �����ڵ� ������ ���� ���̺�
    private const int HANGUL_START = 0xAC00;
    private const int HANGUL_END = 0xD7A3;
    private static readonly string[] JUNGSEONG_TABLE =
    {
        "��", "��", "��", "��", 
        "��", "��", "��", "��",
        "��", "��", "��", "��", 
        "��", "��", "��", "��", 
        "��", "��", "��", "��", "��"
    };
    Dictionary<string, MOUTH> _dicVowel = new Dictionary<string, MOUTH>{ 
        // { "��", MOUTH.A }, { "��", MOUTH.E }, { "��", MOUTH.A }, { "��", MOUTH.A }, 
        // { "��", MOUTH.E }, { "��", MOUTH.E }, { "��", MOUTH.E }, { "��", MOUTH.E },
        // { "��", MOUTH.O }, { "��", MOUTH.O }, { "��", MOUTH.O }, { "��", MOUTH.O },
        // { "��", MOUTH.O }, { "��", MOUTH.U }, { "��", MOUTH.U }, { "��", MOUTH.U },
        // { "��", MOUTH.I }, { "��", MOUTH.U }, { "��", MOUTH.U }, { "��", MOUTH.I }, { "��", MOUTH.I },
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _manager._delActivateItem += OnActivateItem;
        _manager._delTutorialEnd += OnTutorialEnd;
    }
    public void OnClickNext()
    {
        _manager.GoNext();
    }
    void OnActivateItem(TutorialItem item) 
    {
        if (item._status == TutorialStatus.Text)
        {
            if (_coroutine != null) 
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            _panelObj.SetActive(true);

            _name.text = item._name;
            _fullText = item._text;
            _delayRate = item._delayRate;

            //_portrait[0].gameObject.SetActive(item._isShowPicL);
            //_portrait[1].gameObject.SetActive(item._isShowPicR);

            if (item._isShowPicL)
            {
                _talkIndex = 0;
                _avatar[0].SetEmotion(item._emotionL);
                _avatar[0].CloseEye(item._closeEyeL);
            }
            _avatar[0].SetTrigger(item._triggerL);

            if (item._isShowPicR)
            {
                _talkIndex = 1;
                _avatar[1].SetEmotion(item._emotionR);
                _avatar[1].CloseEye(item._closeEyeR);
            }
            _avatar[1].SetTrigger(item._triggerR);

            _coroutine = StartCoroutine(TypeTextWithRichText());
        }
        else 
        {
            _panelObj.SetActive(false);
        }
    }
    void OnTutorialEnd() 
    { 
    }
    // ���ڿ����� �� ������ ������ �����ϴ� �Լ�
    public static string GetVowel(System.Char c)
    {
        /*if (string.IsNullOrEmpty(character) || character.Length != 1)
        {
            Debug.LogError("�Է��� �� ���ڿ��� �մϴ�.");
            return null;
        }

        char c = character[0];
        */
        // �����ڵ� ���� ���� �ִ��� Ȯ��
        if (c < HANGUL_START || c > HANGUL_END)
        {
            Debug.LogWarning("�ѱ��� �ƴ� ���ڰ� �ԷµǾ����ϴ�.");
            return null;
        }

        // �ѱ� �����ڵ� ������ �߼� �ε����� ����
        int unicodeIndex = c - HANGUL_START;
        int jungsungIndex = (unicodeIndex % (21 * 28)) / 28;

        return JUNGSEONG_TABLE[jungsungIndex];
    }
    private IEnumerator TypeTextWithRichText()
    {
        _text.text = "";
        int index = 0;
        string currentText = "";

        while (index < _fullText.Length)
        {
            if (_fullText[index] == '<') // �±� ����
            {
                int closeTagIndex = _fullText.IndexOf('>', index);
                if (closeTagIndex != -1) // �±� ����
                {
                    currentText += _fullText.Substring(index, closeTagIndex - index + 1);
                    index = closeTagIndex + 1;
                    continue;
                }
            }
            else
            {
                var ch = _fullText[index];
                currentText += ch;
                index++;

                var vowel = GetVowel(ch);
                var mouth = MOUTH.None;
                if(vowel != null && _dicVowel.ContainsKey(vowel))
                    mouth = _dicVowel[vowel];

                _avatar[_talkIndex].SetMouth(mouth);

                _text.text = currentText;

                yield return new WaitForSeconds(_textDelay * _delayRate);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
