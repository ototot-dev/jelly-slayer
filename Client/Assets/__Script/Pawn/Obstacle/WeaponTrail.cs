using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponTrail : MonoBehaviour
{
    [SerializeField] TrailRenderer _trail;

    // Start is called before the first frame update
    void Start()
    {
        _trail.enabled = false;
    }
    public void ShowTrail(bool showTrail) 
    {
        _trail.enabled = showTrail;
    }
}
