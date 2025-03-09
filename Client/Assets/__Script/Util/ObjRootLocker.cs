using UnityEngine;

public class ObjRootLocker : MonoBehaviour
{
    [SerializeField]
    public bool _isRootLock = false;
    [SerializeField]
    public Transform _trRoot;

    public Vector3 _vOrigin;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_trRoot != null) 
        {
            _vOrigin = _trRoot.position;
        }        
    }

    // Update is called once per frame
    void Update()
    {
        if (_isRootLock == true) 
        {
            _trRoot.position = _vOrigin;
        }        
    }
}
