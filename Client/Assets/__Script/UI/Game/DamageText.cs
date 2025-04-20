using System;
using Game;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class DamageText : MonoBehaviour, IObjectPoolable
{
    public RectTransform _rtRoot;

    [SerializeField]
    private TextMeshProUGUI _textTmp;
    [SerializeField]
    private Text _text;

    private float _lifeTimeCur = 0;
    [SerializeField]
    private float _lifeTimeMax = 2;

    Vector3 _vWorldPos;
    Color _color = Color.white;

    float _xPos = 0;
    float _yPos = 0;
    float _yAdd = 0;
    float _xAdd = 0;
    float _yMax = 150.0f;
    bool _isYDamp = true;

    public void SetText(string text, Vector3 vPos, float scale, Color color) 
    {
        _text.text = text;
        _lifeTimeCur = _lifeTimeMax;

        _xPos = 0;
        _yPos = 0;
        _yAdd = 0;
        _isYDamp = true;
        _color = color;
        _text.color = _color;

        _yMax = UnityEngine.Random.Range(140, 180);
        _xAdd = UnityEngine.Random.Range(40, 90);
        if (UnityEngine.Random.Range(0, 1000) % 2 == 0) 
        {
            _xAdd *= -1;
        }

        _vWorldPos = vPos;
        _rtRoot.localPosition = GameContext.Instance.damageTextManager.GetCanvasPos(_vWorldPos);
        _rtRoot.localScale = scale * Vector3.one;
    }

    void UpdateInternal()
    {
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
            _yAdd += 1.4f * Time.deltaTime;
        }
        var pos = GameContext.Instance.damageTextManager.GetCanvasPos(_vWorldPos);
        pos.x += _xPos;
        pos.y += _yPos;

        _rtRoot.localPosition = pos;

        // Life Time
        _lifeTimeCur -= Time.deltaTime;
        if (_lifeTimeCur <= 0) 
        {
            ObjectPoolingSystem.Instance.ReturnObject(gameObject);
            return;
        }
        // Alpha
        if (_lifeTimeCur <= 0.5f) 
        {
            _color.a = 2.0f * _lifeTimeCur;
            _text.color = _color;
        }
    }

    IDisposable __updateDisposable;

    public void OnGetFromPool()
    {
        __updateDisposable = Observable.EveryUpdate().Subscribe(_ => UpdateInternal()).AddTo(this);
    }

    public void OnReturnedToPool()
    {
        __updateDisposable?.Dispose();
        __updateDisposable = null;
    }
}
