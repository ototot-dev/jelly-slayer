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

    public void OnEventStartJump() {}
    public void OnEventStartLand() {}
    public void OnEventRollingGround() {}
    public void Hit() { }
    public void FootR() { }
    public void FootL() { }
    public void Land() { }
    public void f_start() { }
    public void f_hit() { }
    public void f_fire() { }
    public void f_afterFire() { }
}
