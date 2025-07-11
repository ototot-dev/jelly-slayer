using System.Linq;
using Game;
using ZLinq;
using UnityEngine;
using UniRx.Triggers.Extension;

public class PawnAnimatorHandler : MonoBehaviour
{
    public PawnAnimController GetAnimController() 
    {
        __animCtrler ??= gameObject.Ancestors().First(a => a.GetComponent<PawnAnimController>() != null).GetComponent<PawnAnimController>();
        return __animCtrler;
    }

    PawnAnimController __animCtrler;

    void OnAnimatorMove()
    {
        if (GetAnimController() != null)
            GetAnimController().OnAnimatorMoveHandler();
    }

    public void OnAnimatorStateEnter(AnimatorStateInfo stateInfo, int layerIndex, PawnAnimStateMachineTrigger trigger)
    {
        if (GetAnimController() != null)
            GetAnimController().OnAnimatorStateEnterHandler(stateInfo, layerIndex, trigger);
    }

    public void OnAniamtorStateExit(AnimatorStateInfo stateInfo, int layerIndex, PawnAnimStateMachineTrigger trigger)
    {
        if (GetAnimController() != null)
            GetAnimController().OnAniamtorStateExitHandler(stateInfo, layerIndex, trigger);
    }

    #region AnimationClip 이벤트 핸들러
    public void OnEventStartJump() { }
    public void OnEventStartLand() { }
    public void OnEventRollingGround() { }
    public void Hit() { }
    public void FootR() { }
    public void FootL() { }
    public void Land() { }
    public void f_start() { }
    public void f_hit() { }
    public void f_fire() { }
    public void f_afterFire() { }
    public void Shoot() { }
    public void StepsEvent() { }
    public void bigShotEvent() { } 
#endregion
}
