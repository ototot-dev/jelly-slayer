using UnityEngine;
using XftWeapon;

public class WeaponController : MonoBehaviour
{
    //[SerializeField] MeleeWeaponTrail _trail;
    [SerializeField] TrailRenderer _trailRender;
    [SerializeField] XWeaponTrail _trailXWeapon;

    // Start is called before the first frame update
    void Start()
    {
        //if (_trail != null)
            //_trail.Emit = false;

        if(_trailRender != null)
        {
            _trailRender.enabled = false;
        }
        if (_trailXWeapon != null)
        {
            //_trailXWeapon.Init();
            _trailXWeapon.Activate();
            //_trailXWeapon.Deactivate();
            Invoke("XWeaponDeactivate", 0.3f);
        }
    }
    void XWeaponDeactivate() 
    {
        if (_trailXWeapon != null)
        {
            _trailXWeapon.Deactivate();
        }
    }
    public void ShowTrail(bool isActive)
    { 
        //if (_trail != null) 
            //_trail.Emit = isActive;
 
        if (_trailRender != null)
        {
            _trailRender.enabled = isActive;
        }
        if (_trailXWeapon != null)
        {
            if (isActive == true)
                _trailXWeapon.Activate();
            else
                _trailXWeapon.Deactivate();
        }
    }
}
