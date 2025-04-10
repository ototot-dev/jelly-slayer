using UnityEngine;
using Game;
using System.Collections.Generic;
using System.Xml;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public enum TutorialStatus 
{ 
    None,
    Text,
    EnableInput,        // �Է� Enable
    DisableInput,       // �Է� Disable
    ActiveTarget,       // ��ü Active
    SetMode,            // ���
    ActiveTargetAttack, // Target Attack Active
    End,                // Ʃ�丮�� ����
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
    Guard,
    Roll,
    Parry,
    Jump,
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

    public float _delayRate = 1;
    public string _text;

    public bool _isShowPicL = false;
    public bool _isShowPicR = false;

    public string _triggerL;
    public string _triggerR;

    public EMOTION _emotionL;
    public EMOTION _emotionR;

    public float _closeEyeL = 0;
    public float _closeEyeR = 0;
}

public class TutorialManager : MonoBehaviour
{
    public PlayerController _playerCtrler;

    public TutorialMode _mode = TutorialMode.None;

    [Serializable]
    public class Count
    {
        public int _normalHit = 0;
        public int _specialHit = 0;
        public int _guard = 0;
        public int _roll = 0;
        public int _jump = 0;
        public int _parry = 0;
    }
    public Count _count = new ();

    [Space(10)]
    public HeroBrain _heroBrain;

    public GameObject[] _targetObj;

    public List<TutorialItem> _list = new();
    int _curItem = -1;

    public delegate void OnActivateItem(TutorialItem item);
    public OnActivateItem _delActivateItem;

    public delegate void OnTutorialEnd();
    public OnTutorialEnd _delTutorialEnd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance._delPawnDamaged += OnPawnDamaged;
        GameManager.Instance._delPawnRolled += OnPawnRolled;
        GameManager.Instance._delPawnJumped += OnPawnJumped;

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

        _playerCtrler.inputLookVec.Value = new Vector3(-1, 0, -1);

        LoadXML();

        StartCoroutine(GetHeroBrain());

        ActivateItem(0);
    }
    private void OnDestroy()
    {
        //GameManager.Instance._delPawnDamaged -= OnPawnDamaged;
    }
    IEnumerator GetHeroBrain() 
    {
        while (_heroBrain == null)
        {
            _heroBrain = _playerCtrler.possessedBrain;

            yield return new WaitForEndOfFrame();
        }
    }
    void LoadXML()
    {
        // XML ���� ��� ����
        //string filePath = Application.dataPath + "/Players.xml";
        TextAsset xmlFile = Resources.Load<TextAsset>("Tutorial/tutorial");
        if (xmlFile == null)
            return;

        // XmlDocument�� XML ���� �б�
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(xmlFile.text);

        // Root ��� ��������
        XmlNodeList tutorialList = xmlDoc.GetElementsByTagName("item");

        // �� Player ���� ���
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

            item._isShowPicL = ParseBool(node["showpic_l"]);
            item._isShowPicR = ParseBool(node["showpic_r"]);

            item._emotionL = ParseEmotion(node["emotion_l"]);
            item._emotionR = ParseEmotion(node["emotion_r"]);

            item._closeEyeL = ParseFloat(node["closeeye_l"]);
            item._closeEyeR = ParseFloat(node["closeeye_r"]);

            item._triggerL = ParseString(node["trigger_l"]);
            item._triggerR = ParseString(node["trigger_r"]);

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
    EMOTION ParseEmotion(XmlElement element)
    {
        if (element == null || element.InnerText.Length <= 0)
        {
            return EMOTION.None;
        }
        return (EMOTION)Enum.Parse(typeof(EMOTION), element.InnerText);
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
                    {
                        var index = item._targetIndex;
                        if (index >= 0 && index < _targetObj.Length)
                        {
                            var obj = _targetObj[index];
                            obj.SetActive(true);

                            EnableSoldierAttack(item._targetIndex, false);
                        }
                        break;
                    }
                case TutorialStatus.ActiveTargetAttack:
                    EnableSoldierAttack(item._targetIndex, true);
                    break;
                case TutorialStatus.End:
                    EndTutorial();
                    break;
            }
            _delActivateItem?.Invoke(item);

            // �ٷ� ����
            if (item._isActivateNextOnStart == true)
            {
                GoNext();
            }
            // ���� �ð� ���� ����
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
    void EndTutorial() 
    {
        SceneManager.LoadScene("Title");
    }
    void EnableSoldierAttack(int index, bool isEnable) 
    {
        if (index < 0 || index >= _targetObj.Length)
            return;
        if (index != 1)
            return;

        var obj = _targetObj[index];
        var brain = obj.GetComponent<SoldierBrain>();
        // brain.debugActionDisabled = !isEnable;

        if (_heroBrain == null)
        {
            _heroBrain = _playerCtrler.possessedBrain;
        }
        _heroBrain.SetTarget(brain);
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
    void OnPawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext) 
    {
        //if(damageContext.senderActionData)
        switch (_mode) 
        {
            case TutorialMode.NormalAttack:
                {
                    _count._normalHit++;
                    if (_count._normalHit >= 3)
                    {
                        SetMode(TutorialMode.None);
                        GoNext();
                    }
                }
                break;
            case TutorialMode.SpecialAttack:
                {
                    if (damageContext.actionResult == ActionResults.GuardBreak)
                    {
                        _count._specialHit++;
                        if (_count._specialHit >= 1)
                        {
                            SetMode(TutorialMode.None);
                            GoNext();
                        }
                    }
                }
                break;
            case TutorialMode.Guard:
                {
                    if (damageContext.actionResult == ActionResults.Blocked && damageContext.receiverBrain == _heroBrain)
                    {
                        _count._guard++;
                        if (_count._guard >= 3)
                        {
                            SetMode(TutorialMode.None);
                            GoNext();
                        }
                    }
                }
                break;
            case TutorialMode.Parry:
                {
                    if (damageContext.actionResult == ActionResults.Blocked && damageContext.receiverBrain == _heroBrain)
                    {
                        _count._parry++;
                        if (_count._parry >= 3)
                        {
                            SetMode(TutorialMode.None);
                            GoNext();
                        }
                    }
                }
                break;
        }
        
    }
    void OnPawnRolled()
    {        
        if (_mode == TutorialMode.Roll)
        {
            _count._roll++;
            if (_count._roll >= 3)
            {
                SetMode(TutorialMode.None);
                GoNext();
            }
        }
    }
    void OnPawnJumped() 
    {
        if (_mode == TutorialMode.Jump)
        {
            _count._jump++;
            if (_count._jump >= 3)
            {
                SetMode(TutorialMode.None);
                GoNext();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_curItem < 0)
        {
            return;
        }
# if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.N))
        {
            if (_mode != TutorialMode.None)
            {
                _mode = TutorialMode.None;
            }
            GoNext();
        }
#endif
    }
}
