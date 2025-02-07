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

    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Instance._delGameStart += OnGameStart;
        GameManager.Instance._delGameEnd += OnGameEnd;
        GameManager.Instance._delPawnSpawned += OnPawnSpawned;
        GameManager.Instance._delPawnDamaged += OnPawnDamaged;
    }
    private void OnDestroy()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance._delGameStart -= OnGameStart;
        GameManager.Instance._delGameEnd -= OnGameEnd;
        GameManager.Instance._delPawnSpawned -= OnPawnSpawned;
        GameManager.Instance._delPawnDamaged -= OnPawnDamaged;
    }
    void OnGameStart() 
    { 
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
            _gamePanel.SetHeroBrain(pawn);
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
        switch(damageContext.receiverPenalty.Item1)
        {
            case PawnStatus.Staggered:
                //_dmgTextManager.Create("Stagger", damageContext.hitPoint);
                break;
            case PawnStatus.KnockDown:
                //_dmgTextManager.Create("KnockDown", damageContext.hitPoint);
                break;
            case PawnStatus.Groggy:
                //_dmgTextManager.Create("Stunned", damageContext.hitPoint);
                break;

        }
        _hpBarManager.PawnDamaged(ref damageContext);

        // 
        switch(damageContext.actionResult) 
        {
            case ActionResults.GuardBreak:
                {
                    Debug.Log("GuardBreak!!!!!!");
                }
                break;
        }
    }
}
