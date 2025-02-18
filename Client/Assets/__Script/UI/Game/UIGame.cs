using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGame : MonoBehaviour
{
    [SerializeField]
    private HPBarManager _hpBarManager;
    [SerializeField]
    private DamageTextManager _dmgTextManager;

    [SerializeField]
    private UIGamePanel _gamePanel;
    [SerializeField]
    private UIGameOver _gameOverPanel;

    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Instance._delGameStart += OnGameStart;
        GameManager.Instance._delGameEnd += OnGameEnd;
        GameManager.Instance._delPawnSpawned += OnPawnSpawned;
        GameManager.Instance._delPawnDamaged += OnPawnDamaged;
        GameManager.Instance._delGameOver += OnGameOver;
    }
    private void OnDestroy()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance._delGameStart -= OnGameStart;
        GameManager.Instance._delGameEnd -= OnGameEnd;
        GameManager.Instance._delPawnSpawned -= OnPawnSpawned;
        GameManager.Instance._delPawnDamaged -= OnPawnDamaged;
        GameManager.Instance._delGameOver -= OnGameOver;
    }
    void OnGameStart() 
    { 
    }
    void OnGameOver() 
    {
        _gameOverPanel.gameObject.SetActive(true);
    }
    void OnGameEnd() 
    {
        _hpBarManager.ClearAll();
    }
    void OnPawnSpawned(PawnBrainController pawn)
    {
        Debug.Log("OnPawnSpawned : " + pawn.name);

        //* Hp 출력안하는 Pawn
        if (pawn is DroneBotBrain)
            return;
        if (pawn is HeroBrain)
        {
            _gamePanel?.SetHeroBrain(pawn);
            return;
        }

            bool isStamina = (pawn.PawnBB.stat.maxStamina.Value > 0);
        _hpBarManager.Create(pawn, isStamina);
    }
    void OnPawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext) 
    {
        Debug.Log("OnPawnDamaged : " + damageContext.finalDamage);

        if (damageContext.finalDamage > 0)
        {
            var isGroggy = damageContext.receiverBrain.PawnBB.IsGroggy;
            Color color = Color.white;
            float scale = 1.0f;

            // Player Attacked
            if (damageContext.receiverBrain.owner != null)
            {
                color = Color.red;
            }
            else
            {
                if (damageContext.insufficientStamina == true)
                {
                    color = Color.grey;
                }
                else if (isGroggy == true) 
                {
                    scale = 1.3f;
                    color = Color.yellow;
                }
            }
            _dmgTextManager.Create(damageContext.finalDamage.ToString("0"), damageContext.hitPoint, scale, color);
        }
        _hpBarManager.PawnDamaged(ref damageContext);
        _gamePanel.PawnDamaged(ref damageContext);
    }
}
