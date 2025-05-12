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

    private Vector3[] velocity = new Vector3[2]; // �ӵ� ���� ����
    public float smoothTime = 0.3f; // ���� �ð� (�������� ������ ����)

    SlayerBrain _brain;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var obj = GameObject.Find("Hero_OneUp");
        if (obj != null)
        {
            _brain = obj.GetComponent<SlayerBrain>();
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

        // SmoothDamp�� �̿��� �̵�
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
