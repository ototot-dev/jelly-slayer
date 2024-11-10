using Game;
using UnityEngine;

public class PawnAnimEvent : MonoBehaviour
{
    [SerializeField]
    PawnBrainController _controller;

    public void OnEventStartJump() 
    {
        Debug.Log("OnEventStartJump");
        if (_controller != null)
        {
            _controller.StartJump();
        }
    }

    public void OnEventStartLand() 
    {
        Debug.Log("OnEventStartLand");
        if (_controller != null) 
        {
            _controller.StartLand();
        }
    }

    public void OnEventRollingGround()
    {
        Debug.Log("OnEventRollingGround");
        if (_controller != null)
        {
            _controller.RollingGround();
        }
    }

    public void Hit() { }
    public void FootR() { }
    public void FootL() { }
    public void f_start() {}
    public void f_hit() {}
    public void f_fire() {}
    public void f_afterFire() {}
}
