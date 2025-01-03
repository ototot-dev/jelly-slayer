using Game;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIGamePanel : MonoBehaviour
{
    [Header("Player")]
    public Slider _playerHpSlider;
    public Slider _playerSpSlider;

    [Header("Enemy")]
    public Slider _enemyHPSlider;
    public Text _enemyName;

    [Space(10)]
    public HeroBrain _heroBrain;
    public PawnBrainController _targetPawn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_heroBrain == null) 
        {
            _heroBrain = FindAnyObjectByType<HeroBrain>();
        }
    }
    public void OnClickMenu() 
    {
        SceneManager.LoadScene("Title");
    }

    // Update is called once per frame
    void Update()
    {
        if (_heroBrain == null)
            return;

        // HP Value
        var maxValue = _heroBrain.PawnBB.stat.maxHeartPoint.Value;
        _playerHpSlider.value = (maxValue > 0) ? _heroBrain.PawnHP.heartPoint.Value / maxValue : 0;

        // Stamina Value
        var maxStamina = _heroBrain.PawnBB.stat.maxStamina.Value;
        _playerSpSlider.value = (maxStamina > 0) ? _heroBrain.PawnBB.stat.stamina.Value / maxStamina : 0;

        // Enemy HP Value
        _targetPawn = _heroBrain.BB.TargetBrain;
        if (_targetPawn != null)
        {
            var maxValue2 = _targetPawn.PawnBB.stat.maxHeartPoint.Value;
            _enemyHPSlider.value = (maxValue > 0) ? _targetPawn.PawnHP.heartPoint.Value / maxValue2 : 0;
        }
        else 
        {
            _enemyHPSlider.value = 0;
        }
    }
}
