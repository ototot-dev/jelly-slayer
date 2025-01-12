using UnityEngine;
using UnityEngine.UI;

public class UITutorialPanel : MonoBehaviour
{
    [SerializeField] TutorialManager _manager;

    [SerializeField] GameObject _panelObj;
    [SerializeField] Text _text;

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
            _panelObj.SetActive(true);
            _text.text = item._text;
        }
        else 
        {
            _panelObj.SetActive(false);
        }
    }
    void OnTutorialEnd() 
    { 
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
