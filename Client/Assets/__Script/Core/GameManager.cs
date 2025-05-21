using Game;
using UnityEngine;

public class GameManager : MonoSingleton<GameManager>
{
    public GameObject SpawnHero(Vector3 vPos)
    {
        if (GameContext.Instance == null || GameContext.Instance.playerCtrler == null) 
            return null;

        var res = Resources.Load<GameObject>("Pawn/Player/Hero_OneUp");
        var pawnObj = GameContext.Instance.playerCtrler.SpawnSlayerPawn(true);
        if (pawnObj == null)
            return null;

        pawnObj.SetActive(true);
        pawnObj.transform.SetPositionAndRotation(vPos, Quaternion.identity);

        OnSpawned(pawnObj.GetComponent<PawnBrainController>());

        return pawnObj;
    }

    public void DespawnHero()
    {
        GameContext.Instance.playerCtrler.possessedBrain.BB.common.isDead.Value = true;
    }

    //public void SpawnDroneBot()
    //{ Instantiate(Resources.Load<GameObject>("Pawn/Player/DroneBot")).transform.SetPositionAndRotation(2f * Vector3.up, Quaternion.identity);}

    //public void SpawnSoldier()
    //{ Instantiate(Resources.Load<GameObject>("Pawn/Jelly/JellySoldier")).transform.SetPositionAndRotation(Vector3.right + Vector3.up, Quaternion.identity);}

    public void DespawnSoldier()
    {
        var soldierBrain = GameObject.FindFirstObjectByType<SoldierBrain>();
        if (soldierBrain != null)
            soldierBrain.BB.common.isDead.Value = true;
    }

    //public void SpawnDroid(Vector3 position)
    //{ Instantiate(Resources.Load<GameObject>("Pawn/Jelly/JellyAlien")).transform.SetPositionAndRotation(position, Quaternion.identity); }

    public void DespawnDroid()
    {
        var alienBrain = GameObject.FindFirstObjectByType<AlienBrain>();
        if (alienBrain != null)
            alienBrain.BB.common.isDead.Value = true;
    }

    //public void SpawnEtasphera42()
    //{ Instantiate(Resources.Load<GameObject>("Pawn/Jelly/JellyEtasphera42")).transform.SetPositionAndRotation(Vector3.right, Quaternion.identity);}

    public void DespawnEtasphera42()
    {
        var etasphera42_brain = GameObject.FindFirstObjectByType<Etasphera42_Brain>();
        if (etasphera42_brain != null)
            etasphera42_brain.BB.common.isDead.Value = true;
    }

    public void ShowLevel_HackerDen(bool isShow)
    {
        // GameObject.Find("Launcher").GetComponent<Launcher>().hackerDen?.SetActive(isShow);
    }

    public void ShowLevel_ShootingRange(bool isShow)
    {
        // GameObject.Find("Launcher").GetComponent<Launcher>().shootingRange?.SetActive(isShow);
    }

    public void ShowLevel_TrainingRoom(bool isShow)
    {
        // GameObject.Find("Launcher").GetComponent<Launcher>().trainingRoom?.SetActive(isShow);
    }

    public void Activate_Title(bool isActive)
    {
        var launcher = GameObject.Find("Launcher").GetComponent<Launcher>();
        // Activate_ObjList(isActive, launcher._objTitleList);
    }
    public void Activate_Game(bool isActive)
    {
        var launcher = GameObject.Find("Launcher").GetComponent<Launcher>();
        // Activate_ObjList(isActive, launcher._objGameList);
    }
    public void Activate_Tutorial(bool isActive)
    {
        var launcher = GameObject.Find("Launcher").GetComponent<Launcher>();
        // Activate_ObjList(isActive, launcher._objTutorialList);
    }

    void Activate_ObjList(bool isActive, GameObject[] objList) 
    { 
        foreach (var obj in objList) 
        { 
            obj.SetActive(isActive);
        }
    }

    public void HideCurrentLevel()
    {
        var launcherObj = GameObject.Find("Launcher");
        var launcher = launcherObj.GetComponent<Launcher>();

        // launcher.hackerDen.SetActive(false);
        // launcher.shootingRange.SetActive(false);
    }

    // 주석    
    public delegate void OnGameStart();
    public OnGameStart _delGameStart;

    public delegate void OnGameEnd();
    public OnGameEnd _delGameEnd;

    public delegate void OnGameOver();
    public OnGameEnd _delGameOver;

    public delegate void OnPawnSpawned(PawnBrainController pawn);
    public OnPawnSpawned _delPawnSpawned;

    public delegate void OnPawnDamaged(ref PawnHeartPointDispatcher.DamageContext damageContext);
    public OnPawnDamaged _delPawnDamaged;

    public delegate void OnPawnRolled();
    public OnPawnRolled _delPawnRolled;

    public delegate void OnPawnJumped();
    public OnPawnJumped _delPawnJumped;

    public bool _isGameStart = false;
    public bool _isGameOver = false;
    public Vector3 _vInitPos;

    public PawnBrainController _player;

    private ObjManager _objManager;

    private void Awake()
    {
        _objManager = gameObject.AddComponent<ObjManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }
    public void StartGame() 
    {
        _isGameStart = true;
        _isGameOver = false;

        _delGameStart?.Invoke();
    }
    public void CloseGame()
    {
        _isGameStart = false;
        _isGameOver = false;

        GameContext.Instance.playerCtrler.Unpossess();

        Activate_Game(false);
        ShowLevel_HackerDen(false);
        ShowLevel_ShootingRange(false);

        //DespawnHero();
        //DespawnEtasphera42();
        //DespawnSoldier();

        _delGameEnd?.Invoke();
    }
    public void OnSpawned(PawnBrainController pawn) 
    { 
        if(pawn == null) return;

        if (pawn is SlayerBrain)
        {
        }
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
    void GameOver() 
    {
        _isGameOver = true;

        _delGameOver?.Invoke();
    }
    private void Update()
    {
        if (_isGameStart == false || _isGameOver == true)
            return;

        var brain = GameContext.Instance.playerCtrler.possessedBrain;
        if (brain != null && brain.PawnBB.IsDead == true) 
        {
            GameOver();
        }
    }
}
