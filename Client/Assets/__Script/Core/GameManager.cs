using Game;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    // 주석    
    public delegate void OnPawnSpawned(PawnBrainController pawn);
    public OnPawnSpawned _delPawnSpawned;

    public delegate void OnPawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext);
    public OnPawnDamaged _delPawnDamaged;

    public delegate void OnPawnRolled();
    public OnPawnRolled _delPawnRolled;

    public delegate void OnPawnJumped();
    public OnPawnJumped _delPawnJumped;

    public Vector3 _vInitPos;

    // Start is called before the first frame update
    void Start()
    {
        return;
        
        var objRes = Resources.Load("Background/Prefabs/Map/FS_Test_Mission2");
        GameObject objBack = (GameObject)GameObject.Instantiate(objRes);
        objBack.transform.position = _vInitPos; // new Vector3(-10.5f, 0, -7.5f);
        objBack.transform.eulerAngles = new Vector3(0, 10, 0);

        var objPlane = GameObject.Find("Plane");
        if(objPlane != null)
        {
            objPlane.SetActive(false);
        }
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
    public void PawnRolled() 
    {
        _delPawnRolled?.Invoke();
    }
    public void PawnJumped()
    {
        _delPawnJumped?.Invoke();
    }
}
