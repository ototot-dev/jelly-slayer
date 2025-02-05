using Game;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public void SpawnHero()
    {
        GameContext.Instance.playerCtrler.SpawnHeroPawn(Resources.Load<GameObject>("Pawn/Player/Hero_OneUp"), true).transform.SetPositionAndRotation(Vector3.left, Quaternion.identity);
    }

    public void DespawnHero()
    {
        GameContext.Instance.playerCtrler.possessedBrain.BB.common.isDead.Value = true;
    }

    public void SpawnDroneBot()
    {
        Instantiate(Resources.Load<GameObject>("Pawn/Player/DroneBot")).transform.SetPositionAndRotation(2f * Vector3.up, Quaternion.identity);
    }

    public void SpawnSoldier()
    {
        Instantiate(Resources.Load<GameObject>("Pawn/Jelly/JellySoldier")).transform.SetPositionAndRotation(Vector3.right, Quaternion.identity);
    }

    public void DespawnSoldier()
    {
        var soldierBrain = GameObject.FindFirstObjectByType<SoldierBrain>();
        if (soldierBrain != null)
            soldierBrain.BB.common.isDead.Value = true;
    }

    public void SpawnEtasphera42()
    {
        Instantiate(Resources.Load<GameObject>("Pawn/Jelly/JellyEtasphera42")).transform.SetPositionAndRotation(Vector3.right, Quaternion.identity);
    }

    public void DespawnEtasphera42()
    {
        var etasphera42_brain = GameObject.FindFirstObjectByType<Etasphera42_Brain>();
        if (etasphera42_brain != null)
            etasphera42_brain.BB.common.isDead.Value = true;
    }

    public void ShowLevel_HackerDen()
    {
        GameObject.Find("Launcher").GetComponent<Launcher>().hackerDen.SetActive(true);
    }

    public void ShowLevel_ShootingRange()
    {
        GameObject.Find("Launcher").GetComponent<Launcher>().shootingRange.SetActive(true);
    }

    public void HideCurrentLevel()
    {
        GameObject.Find("Launcher").GetComponent<Launcher>().hackerDen.SetActive(false);
        GameObject.Find("Launcher").GetComponent<Launcher>().shootingRange.SetActive(false);
    }

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
