using System;
using System.Threading;
using UniRx;
using UnityEngine;

public class TestAnimController : MonoBehaviour
{

    public Animator animCtrler;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animCtrler.SetTrigger("onTest");
            animCtrler.SetLayerWeight(2, 1f);
            Observable.Timer(TimeSpan.FromSeconds(2f))
                .Subscribe(_ => animCtrler.SetLayerWeight(2, 0f));
        }
    }
}
