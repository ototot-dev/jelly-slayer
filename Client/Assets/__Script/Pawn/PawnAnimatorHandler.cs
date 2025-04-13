using System.Linq;
using Game;
using Unity.Linq;
using UnityEngine;

public class PawnAnimatorHandler : MonoBehaviour
{
    PawnAnimController __animCtrler;
    
    void OnAnimatorMove()
    {
        __animCtrler ??= gameObject.Ancestors().First(a => a.GetComponent<PawnAnimController>() != null).GetComponent<PawnAnimController>();
        __animCtrler.OnAnimatorMoveHandler();
    }
    public void OnAnimatorStateEnter(AnimatorStateInfo stateInfo, int layerIndex)
    {
        __animCtrler ??= gameObject.Ancestors().First(a => a.GetComponent<PawnAnimController>() != null).GetComponent<PawnAnimController>();
        __animCtrler.OnAnimatorStateEnterHandler(stateInfo, layerIndex);
    }
    public void OnAniamtorStateExit(AnimatorStateInfo stateInfo, int layerIndex)
    {
        __animCtrler ??= gameObject.Ancestors().First(a => a.GetComponent<PawnAnimController>() != null).GetComponent<PawnAnimController>();
        __animCtrler.OnAniamtorStateExitHandler(stateInfo, layerIndex);
    }

    public void OnEventStartJump() {}
    public void OnEventStartLand() {}
    public void OnEventRollingGround() {}
    public void Hit() { }
    public void FootR() 
    {
        __animCtrler?.OnAnimatorFootHandler(true);
    }
    public void FootL() 
    {
        __animCtrler?.OnAnimatorFootHandler(false);
    }
    public void Land() { }
    public void f_start() { }
    public void f_hit() { }
    public void f_fire() { }
    public void f_afterFire() { }
    public void Shoot() {}
}
