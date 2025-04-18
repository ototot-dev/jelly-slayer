using Game;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UniRx;
using Unity.Linq;
using UnityEngine;

[System.Serializable]
public class SpawnData
{
    public PawnId _id;
    public bool _isEnable = true;
    public float _delay;
    public Vector3 _pos;
}

public class SpawnObject : MonoBehaviour
{
    public SpawnData[] _spawnData;

    public List<GameObject> _pawnList = new ();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => GameManager.Instance.SpawnHero(new UnityEngine.Vector3(-2, 0, -3)));
        //Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => GameManager.Instance.SpawnDroneBot());
        //Observable.Timer(TimeSpan.FromSeconds(2.0f)).Subscribe(_ => GameManager.Instance.SpawnSoldier());
        //Observable.Timer(TimeSpan.FromSeconds(3.0f)).Subscribe(_ => GameManager.Instance.SpawnDroid(UnityEngine.Vector3.right * 5));
        //Observable.Timer(TimeSpan.FromSeconds(4.0f)).Subscribe(_ => GameManager.Instance.SpawnDroid(UnityEngine.Vector3.left * 5));
    }
    private void OnEnable()
    {
        int count = _spawnData.Length;
        for (int ia=0; ia<count; ia++) 
        {
            var data = _spawnData[ia];
            if (data._isEnable == true)
            {
                Observable.Timer(TimeSpan.FromSeconds(data._delay)).Subscribe(_ =>
                {
                    Spawn(data._id, data._pos);
                });
            }
        }
    }
    private void OnDisable()
    {
        int count = _pawnList.Count;
        for (int ia=0; ia<count; ia++)
        {
            var obj = _pawnList[ia];
            if (obj)
            {
                var controller = obj.GetComponent<PawnBrainController>();
                if(controller != null)
                {
                    controller.PawnBB.common.isDead.Value = true;
                }
                obj.Destroy();
            }
        }
        _pawnList.Clear();
    }
    void Spawn(PawnId id, Vector3 pos)
    {
        switch (id)
        {
            case PawnId.Hero:
                SpawnHero(pos);
                break;
            case PawnId.DroneBot:
                SpawnPawn("Pawn/Player/DroneBot", pos);
                break;
            case PawnId.Soldier:
                SpawnPawn("Pawn/Jelly/JellySoldier", pos);
                break;
            case PawnId.Alien:
                SpawnPawn("Pawn/Jelly/JellyAlien", pos);
                break;
            case PawnId.Etasphera42:
                SpawnPawn("Pawn/Jelly/JellyEtasphera42", pos);
                break;
            case PawnId.RoboDog:
                SpawnPawn("Pawn/Jelly/JellyRoboDog", pos);
                break;
        }
    }
    GameObject SpawnHero(Vector3 pos) 
    {
        var pawnObj = GameManager.Instance.SpawnHero(pos);

        _pawnList.Add(pawnObj);
        return pawnObj;
    }
    GameObject SpawnPawn(string resPath, Vector3 pos) 
    {
        var resObj = Resources.Load<GameObject>(resPath);
        if (resObj == null)
            return null;

        var pawnObj = Instantiate(resObj);
        pawnObj.transform.SetPositionAndRotation(pos, Quaternion.identity);

        _pawnList.Add(pawnObj);

        return pawnObj;
    }
    public void SpawnDroneBot(Vector3 pos)
    {
        SpawnPawn("Pawn/Player/DroneBot", 2f * Vector3.up);
    }

    public void SpawnSoldier()
    {
        var pawnObj = Instantiate(Resources.Load<GameObject>("Pawn/Jelly/JellySoldier"));
        pawnObj.transform.SetPositionAndRotation(Vector3.right + Vector3.up, Quaternion.identity);
    }

    public void SpawnDroid(Vector3 position)
    {
        Instantiate(Resources.Load<GameObject>("Pawn/Jelly/JellyAlien")).transform.SetPositionAndRotation(position, Quaternion.identity);
    }
}
