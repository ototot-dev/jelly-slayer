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

    // 한글 유니코드 범위와 모음 테이블
    private const int HANGUL_START = 0xAC00;
    private const int HANGUL_END = 0xD7A3;
    private static readonly string[] JUNGSEONG_TABLE =
    {
        "ㅏ", "ㅐ", "ㅑ", "ㅒ", 
        "ㅓ", "ㅔ", "ㅕ", "ㅖ",
        "ㅗ", "ㅘ", "ㅙ", "ㅚ", 
        "ㅛ", "ㅜ", "ㅝ", "ㅞ", 
        "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ"
    };
    Dictionary<string, MOUTH> _dicVowel = new Dictionary<string, MOUTH>{ 
        { "ㅏ", MOUTH.A }, { "ㅐ", MOUTH.E }, { "ㅑ", MOUTH.A }, { "ㅒ", MOUTH.A }, 
        { "ㅓ", MOUTH.E }, { "ㅔ", MOUTH.E }, { "ㅕ", MOUTH.E }, { "ㅖ", MOUTH.E },
        { "ㅗ", MOUTH.O }, { "ㅘ", MOUTH.O }, { "ㅙ", MOUTH.O }, { "ㅚ", MOUTH.O },
        { "ㅛ", MOUTH.O }, { "ㅜ", MOUTH.U }, { "ㅝ", MOUTH.U }, { "ㅞ", MOUTH.U },
        { "ㅟ", MOUTH.I }, { "ㅠ", MOUTH.U }, { "ㅡ", MOUTH.U }, { "ㅢ", MOUTH.I }, { "ㅣ", MOUTH.I },

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
    // 문자열에서 한 글자의 모음을 추출하는 함수
    public static string GetVowel(System.Char c)
    {
        /*if (string.IsNullOrEmpty(character) || character.Length != 1)
        {
            Debug.LogError("입력은 한 글자여야 합니다.");
            return null;
        }

        char c = character[0];
        */
        // 유니코드 범위 내에 있는지 확인
        if (c < HANGUL_START || c > HANGUL_END)
        {
            Debug.LogWarning("한글이 아닌 문자가 입력되었습니다.");
            return null;
        }

        // 한글 유니코드 값에서 중성 인덱스를 추출
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
            if (_fullText[index] == '<') // 태그 시작
            {
                int closeTagIndex = _fullText.IndexOf('>', index);
                if (closeTagIndex != -1) // 태그 닫힘
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
