using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBarPanel : MonoBehaviour
{
    public RectTransform _rtRoot;
    public RectTransform _rtCanvas;

    public PawnBrainController _pawn;

    [Header("Health Bar")]
    public Slider _hpSlider;

    [Header("Stamina Bar")]
    [SerializeField] bool _isStamina = false;
    public Slider _spSlider;

    //Vector2 _pos;

    bool _isDead = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void SetPawn(PawnBrainController pawn, bool isStamina = false) 
    {
        _pawn = pawn;

        _isStamina = isStamina;
        _spSlider.gameObject.SetActive(_isStamina);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(_pawn == null || _pawn.PawnBB == null) 
            return;
        if (_isDead == true) 
            return;
 
        if (_isDead == false)
        {
            if (_pawn.PawnBB.IsDead == true)
            {
                _isDead = true;
                gameObject.SetActive(false);
                return;
            }
        }

        if (GameContext.Instance.cameraCtrler == null)
            return;

        float rate = 2.5f * (GameContext.Instance.cameraCtrler.zoom / 20.0f);
        rate = Mathf.Max(rate, 2.5f);
        rate = Mathf.Min(rate, 4.0f);
        var pos = _pawn.coreColliderHelper.transform.position + (rate * Vector3.up);

        Vector2 viewportPosition = GameContext.Instance.cameraCtrler.viewCamera.WorldToViewportPoint(pos);
        Vector2 screenPosition = new Vector2(
            ((viewportPosition.x * _rtCanvas.sizeDelta.x) - (_rtCanvas.sizeDelta.x * 0.5f)),
            ((viewportPosition.y * _rtCanvas.sizeDelta.y) - (_rtCanvas.sizeDelta.y * 0.5f)));

        _rtRoot.anchoredPosition = screenPosition;

        // HP Value
        var maxValue = _pawn.PawnBB.stat.maxHeartPoint.Value;
        _hpSlider.value = (maxValue > 0) ?  _pawn.PawnHP.heartPoint.Value / maxValue : 0;

        // Stamina Value
        if(_isStamina == true)
        {
            var maxStamina = _pawn.PawnBB.stat.maxStamina.Value;
            _spSlider.value = (maxStamina > 0) ? _pawn.PawnBB.stat.stamina.Value / maxStamina : 0;
        }
    }
}