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

    // Start is called before the first frame update
    void Awake()
    {
        GameManager.Instance._delPawnSpawned += OnPawnSpawned;
        GameManager.Instance._delPawnDamaged += OnPawnDamaged;
    }
    private void OnDestroy()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance._delPawnSpawned -= OnPawnSpawned;
        GameManager.Instance._delPawnDamaged -= OnPawnDamaged;
    }
    void OnPawnSpawned(PawnBrainController pawn)
    {
        Debug.Log("OnPawnSpawned : " + pawn.name);

        bool isStamina = (pawn.PawnBB.stat.maxStamina.Value > 0);
        _hpBarManager.Create(pawn, isStamina);
    }
    void OnPawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext) 
    {
        Debug.Log("OnPawnDamaged : " + damageContext.finalDamage);

        if (damageContext.finalDamage > 0)
        {
            _dmgTextManager.Create(damageContext.finalDamage.ToString(), damageContext.hitPoint);
        }
        switch(damageContext.receiverPenalty.Item1)
        {
            case BuffTypes.Staggered:
                //_dmgTextManager.Create("Stagger", damageContext.hitPoint);
                break;
            case BuffTypes.KnockDown:
                _dmgTextManager.Create("KnockDown", damageContext.hitPoint);
                break;
            case BuffTypes.Groggy:
                _dmgTextManager.Create("Stunned", damageContext.hitPoint);
                break;

        }
        // 액션 상태
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
