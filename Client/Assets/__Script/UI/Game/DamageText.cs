using FlowCanvas.Nodes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public DamageTextManager Manager { get; set; }

    public RectTransform _rtRoot;
    [SerializeField]
    private TextMeshProUGUI _text;

    private float _lifeTimeCur = 0;
    [SerializeField]
    private float _lifeTimeMax = 2;

    Vector3 _vWorldPos;
    Color _color = Color.white;

    float _xPos = 0;
    float _yPos = 0;
    float _yAdd = 0;
    float _xAdd = 0;
    float _yMax = 150.0f; // �ִ� ����
    bool _isYDamp = true;

    bool _isDie = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetText(string text, Vector3 vPos) 
    {
        _text.text = text;
        _lifeTimeCur = _lifeTimeMax;

        _xPos = 0;
        _yPos = 0;
        _yAdd = 0;
        _isDie = false;
        _isYDamp = true;
        _color = Color.white;
        _text.color = _color;

        gameObject.SetActive(true);

        _yMax = UnityEngine.Random.Range(140, 240);
        _xAdd = UnityEngine.Random.Range(90, 180);
        if (UnityEngine.Random.Range(0, 1000) % 2 == 0) 
        {
            _xAdd *= -1;
        }
        _vWorldPos = vPos;
        _rtRoot.localPosition = Manager.GetCanvasPos(_vWorldPos);
    }
    void Die() 
    {
        if (_isDie == true)
            return;

        _isDie = true;
        gameObject.SetActive(false);

        Manager.Die(this);
        //Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        if (_isDie == true) 
            return;

        _xPos += _xAdd * Time.deltaTime;
        _xAdd -= 0.01f * _xAdd;

        if (_isYDamp == true)
        {
            _yPos = Mathf.SmoothDamp(_yPos, _yMax, ref _yAdd, 0.15f);
            if (_yPos >= (_yMax - 5.0f))
            {
                _isYDamp = false;
                _yAdd = 0;
            }
        }
        else
        {
            _yPos -= _yAdd;
            _yAdd += 2.0f * Time.deltaTime;
        }
        var pos = Manager.GetCanvasPos(_vWorldPos);
        pos.x += _xPos;
        pos.y += _yPos;

        _rtRoot.localPosition = pos;

        // ����
        _lifeTimeCur -= Time.deltaTime;
        if (_lifeTimeCur <= 0) 
        {
            Die();
            return;
        }
        // Alpha
        if (_lifeTimeCur <= 1.0f) 
        {
            _color.a = _lifeTimeCur;
            _text.color = _color;
        }
    }
}