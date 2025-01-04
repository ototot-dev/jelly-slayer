using Game;
using UnityEngine;

public class DoorObj : MonoBehaviour
{
    [SerializeField]
    Transform _trRoot;

    [SerializeField]
    Transform[] _trDoor;
    [SerializeField]
    Vector3[] _targetPos;

    [SerializeField]
    bool _isOpen = false;

    private Vector3[] velocity = new Vector3[2]; // 속도 저장 변수
    public float smoothTime = 0.3f; // 감속 시간 (작을수록 빠르게 멈춤)

    HeroBrain _brain;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var obj = GameObject.Find("Hero_OneUp");
        if (obj != null)
        {
            _brain = obj.GetComponent<HeroBrain>();
        }
    }
    public void Open() 
    {
        _isOpen = true;
    }
    public void Close() 
    { 
    }
    // Update is called once per frame
    void Update()
    {
        if(_brain == null)
            return;

        var vDist = _trRoot.position - _brain.coreColliderHelper.transform.position;
        _isOpen = (vDist.magnitude <= 3.5f);

        // SmoothDamp를 이용해 이동
        for (int ia = 0; ia < 2; ia++)
        {
            var targetPos = (_isOpen == true) ? _targetPos[ia] : new Vector3(-2.4f, 0, 0);
            _trDoor[ia].localPosition = Vector3.SmoothDamp(
                _trDoor[ia].localPosition,
                targetPos,
                ref velocity[ia],
                smoothTime
            );
        }
    }
}
