using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UITutorialPanel : MonoBehaviour
{
    [SerializeField] TutorialManager _manager;

    [SerializeField] GameObject _panelObj;
    [SerializeField] Text _text;

    [SerializeField] RawImage[] _portrait;

    [SerializeField] RenderTexture _renderTex;

    int _curCursor = 0;
    string _fullText;

    public float _textDelay = 0.03f;
    float _delayRate =  1.0f;

    Coroutine _coroutine;

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
            _fullText = item._text;
            _delayRate = item._delayRate;

            _portrait[0].gameObject.SetActive(item._isShowPortraitL);
            _portrait[1].gameObject.SetActive(item._isShowPortraitR);

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
    private IEnumerator TypeTextWithRichText()
    {
        _text.text = "";
        int index = 0;
        string currentText = "";

        while (index < _fullText.Length)
        {
            if (_fullText[index] == '<') // ÅÂ±× ½ÃÀÛ
            {
                int closeTagIndex = _fullText.IndexOf('>', index);
                if (closeTagIndex != -1) // ÅÂ±× ´ÝÈû
                {
                    currentText += _fullText.Substring(index, closeTagIndex - index + 1);
                    index = closeTagIndex + 1;
                    continue;
                }
            }
            else
            {
                currentText += _fullText[index];
                index++;

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
