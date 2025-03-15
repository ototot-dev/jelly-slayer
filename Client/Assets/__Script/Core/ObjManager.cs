using Game;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjManager : MonoBehaviour
{
    static ObjManager _instance;
    public static ObjManager Instance {
        get { return _instance; }
    }
    [SerializeField]
    List<PawnBrainController> _jellyList = new();

    private void Awake()
    {
        _instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PawnBrainController[] brains = FindObjectsByType<PawnBrainController>(FindObjectsSortMode.None);
        foreach (var brain in brains) {
            OnSpawned(brain);
        }
    }

    public List<PawnBrainController> GetJellyList() {
        return _jellyList;
    }

    public void OnSpawned(PawnBrainController pawn)
    {
        if (pawn == null) 
            return;

        var pawnTag = pawn.tag;
        if (pawnTag == "Jelly")
        {
            if (_jellyList.Contains(pawn))
                return;

            _jellyList.Add(pawn);
        }
    }
}
