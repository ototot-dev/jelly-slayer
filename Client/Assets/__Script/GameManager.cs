using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{

    // 주석
    
    public delegate void OnPawnSpawned(PawnBrainController pawn);
    public OnPawnSpawned _delPawnSpawned;

    public delegate void OnPawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext);
    public OnPawnDamaged _delPawnDamaged;


    // Start is called before the first frame update
    void Start()
{
        
    }
    public void Spawn(PawnBrainController pawn) 
    { 
        _delPawnSpawned?.Invoke(pawn);
    }
    public void PawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext) 
    {
        //if (damageContext.finalDamage <= 0)
            //return;

        _delPawnDamaged?.Invoke(ref damageContext);
    }
}
