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

    /// <summary>
    /// ���� �ÿ� ȣ��
    /// </summary>
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

}
