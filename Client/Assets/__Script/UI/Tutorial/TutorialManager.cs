using UnityEngine;
using Game;
using System.Collections.Generic;
using System.Xml;
using System;
using System.Linq;
using NodeCanvas.StateMachines;

public enum TutorialStatus 
{ 
    None,
    Text,
    EnableInput, // 입력 Enable
    DisableInput, // 입력 Disable
    ActiveTarget, // 객체 Active
    SetMode,
}

public enum TutorialAction 
{
    None,
    Look,
    Move,
    Guard,
    Jump,
    NormalAttack,
    SpecialAttack,
    Parry,
    Roll,
}

public enum TutorialMode 
{
    None,
    NormalAttack,
    SpecialAttack,
}

[System.Serializable]
public class TutorialItem 
{
    public string _name;

    public TutorialStatus _status;
    public TutorialAction _action;
    public TutorialMode _mode;
    public bool _isActivateNextOnStart = false;
    public float _nextActivateTime = 0;
    public int _targetIndex = 0;

    public string _text;
}

public class TutorialManager : MonoBehaviour
{
    public PlayerController _playerCtrler;

    public TutorialMode _mode = TutorialMode.None;
    public List<TutorialItem> _list = new();
    int _curItem = -1;

    public GameObject[] _targetObj;

    public delegate void OnActivateItem(TutorialItem item);
    public OnActivateItem _delActivateItem;

    public delegate void OnTutorialEnd();
    public OnTutorialEnd _delTutorialEnd;

    public int _normalHitCount = 0;
    public int _specialHitCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance._delPawnDamaged += OnPawnDamaged;

        if (_playerCtrler == null)
        {
            _playerCtrler = FindFirstObjectByType<PlayerController>();
        }
        _playerCtrler._isEnable_Move = false;
        _playerCtrler._isEnable_Look = false;
        _playerCtrler._isEnable_Guard = false;
        _playerCtrler._isEnable_Jump = false;
        _playerCtrler._isEnable_NormalAttack = false;
        _playerCtrler._isEnable_Parry = false;
        _playerCtrler._isEnable_Roll = false;
        _playerCtrler._isEnable_SpecialAttack = false;

        LoadXML();

        ActivateItem(0);
    }
    private void OnDestroy()
    {
        GameManager.Instance._delPawnDamaged -= OnPawnDamaged;
    }
    void LoadXML()
    {
        // XML 파일 경로 설정
        //string filePath = Application.dataPath + "/Players.xml";
        TextAsset xmlFile = Resources.Load<TextAsset>("Tutorial/tutorial");
        if (xmlFile == null)
            return;

        // XmlDocument로 XML 파일 읽기
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlFile.text);

        // Root 노드 가져오기
        XmlNodeList tutorialList = xmlDoc.GetElementsByTagName("item");

        // 각 Player 정보 출력
        foreach (XmlNode node in tutorialList)
        {
            TutorialItem item = new();

            item._status = ParseStatus(node["status"]);
            item._action = ParseAction(node["action"]);
            item._mode = ParseMode(node["mode"]);

            item._name = ParseString(node["name"]);
            item._text = ParseString(node["text"]);
            item._isActivateNextOnStart = ParseBool(node["activatenext_onstart"]);
            item._nextActivateTime = ParseFloat(node["activatenext_aftertime"]);

            item._targetIndex = ParseInt(node["targetindex"]);

            _list.Add(item);
        }
    }
    string ParseString(XmlElement element)
    {
        if (element == null || element.InnerText.Length <= 0)
            return "";

        return element.InnerText;
    }
    float ParseFloat(XmlElement element)
    {
        if (element == null || element.InnerText.Length <= 0)
            return 0;

        return float.Parse(element.InnerText);
    }
    bool ParseBool(XmlElement element)
    {
        if (element == null || element.InnerText.Length <= 0)
            return false;

        return bool.Parse(element.InnerText);
    }
    int ParseInt(XmlElement element)
    {
        if (element == null || element.InnerText.Length <= 0)
            return 0;

        return int.Parse(element.InnerText);
    }
    TutorialStatus ParseStatus(XmlElement element)
    {
        if (element == null || element.InnerText.Length <= 0)
        {
            return TutorialStatus.None;
        }
        return (TutorialStatus)Enum.Parse(typeof(TutorialStatus), element.InnerText);
    }
    TutorialAction ParseAction(XmlElement element)
    {
        if (element == null || element.InnerText.Length <= 0)
        {
            return TutorialAction.None;
        }
        return (TutorialAction)Enum.Parse(typeof(TutorialAction), element.InnerText);
    }
    TutorialMode ParseMode(XmlElement element)
    {
        if (element == null || element.InnerText.Length <= 0)
        {
            return TutorialMode.None;
        }
        return (TutorialMode)Enum.Parse(typeof(TutorialMode), element.InnerText);
    }
    void ActivateItem(int itemIndex)
    {
        _curItem = itemIndex;

        if (_curItem < _list.Count)
        {
            var item = _list[_curItem];

            switch (item._status) {
                case TutorialStatus.EnableInput:
                    EnableInput(item, true);
                    break;
                case TutorialStatus.DisableInput:
                    EnableInput(item, false);
                    break;
                case TutorialStatus.SetMode:
                    SetMode(item._mode);
                    break;
                case TutorialStatus.ActiveTarget:
                    if (item._targetIndex >= 0 && item._targetIndex < _targetObj.Length)
                    {
                        var index = item._targetIndex;
                        var obj = _targetObj[index];
                        obj.SetActive(true);

                        switch (index) 
                        {
                            case 1:
                                {
                                    //var brain = obj.GetComponent<SoldierBrain>();
                                    //brain.JellyBB.decision.aggressiveLevel.Value = -1f;
                                    //brain.InvalidateDecision(0.5f);
                                }
                                break;
                        }
                    }
                    break;
            }
            _delActivateItem?.Invoke(item);

            // 바로 시작
            if (item._isActivateNextOnStart == true)
            {
                GoNext();
            }
            // 일정 시간 이후 시작
            else if (item._nextActivateTime > 0)
            {
                Invoke(nameof(GoNext), item._nextActivateTime);
            }
        }
        else
        {
            _delTutorialEnd?.Invoke();
        }
    }
    void SetMode(TutorialMode mode) 
    {
        _mode = mode;
    }

    void EnableInput(TutorialItem item, bool isEnable = true) 
    {
        switch (item._action)
        {
            case TutorialAction.Look:
                _playerCtrler._isEnable_Look = isEnable; break;
            case TutorialAction.Move:
                _playerCtrler._isEnable_Move = isEnable; break;
            case TutorialAction.Guard:
                _playerCtrler._isEnable_Guard = isEnable; break;
            case TutorialAction.Jump:
                _playerCtrler._isEnable_Jump = isEnable; break;
            case TutorialAction.NormalAttack:
                _playerCtrler._isEnable_NormalAttack = isEnable; break;
            case TutorialAction.SpecialAttack:
                _playerCtrler._isEnable_SpecialAttack = isEnable; break;
            case TutorialAction.Parry:
                _playerCtrler._isEnable_Parry = isEnable; break;
            case TutorialAction.Roll:
                _playerCtrler._isEnable_Roll = isEnable; break;
        }
    }
    public void GoNext()
    {
        ActivateItem(_curItem + 1);
    }
    public void OnPawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext) 
    {
        //if(damageContext.senderActionData)
        switch (_mode) 
        {
            case TutorialMode.NormalAttack:
                {
                    _normalHitCount++;
                    if (_normalHitCount >= 3)
                    {
                        SetMode(TutorialMode.None);
                        GoNext();
                    }
                }
                break;
            case TutorialMode.SpecialAttack:
                {
                    _specialHitCount++;
                    if (_specialHitCount >= 1)
                    {
                        SetMode(TutorialMode.None);
                        GoNext();
                    }
                }
                break;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_curItem < 0)
        {
            return;
        }

        switch (_mode) 
        {
            case TutorialMode.NormalAttack:
                {
                    
                }
                break;
        }
    }
}
