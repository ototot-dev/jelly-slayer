using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITItleButton : MonoBehaviour
{
    public Transform _trBack;
    public Text _text;

    public float _targetScale = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void Select(bool isSelect) 
    {
        float scale = isSelect ? 1.2f : 1f;
        DOTween.To(() => _targetScale, x => _targetScale = x, scale, 0.2f)
            .OnUpdate(() => _trBack.localScale = _targetScale * Vector3.one);

        _text.fontStyle = isSelect ? FontStyle.Bold : FontStyle.Normal;
    }
}
