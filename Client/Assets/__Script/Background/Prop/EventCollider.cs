using Game;
using UnityEngine;

public class EventCollider : MonoBehaviour
{
    [SerializeField] bool _isEventDestroy = false;
    [SerializeField] bool _isEventToGameMode = false;
    [SerializeField] GameObject _targetObj = null;

    [SerializeField] string _message = "";
    [SerializeField] int _param = 0;

    int _eventCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {    
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent != null && other.transform.parent.tag == "Hero") 
        { 
            DoEvent();
        }
    }
    void DoEvent() 
    {
        if (_isEventToGameMode == true)
        {
            var mode = GameContext.Instance.launcher.currGameMode;
            if (mode != null) 
            {
                mode.SendMessage(_message, _param, SendMessageOptions.DontRequireReceiver);
            }
        }
        else if(_targetObj != null)
        {
            _targetObj.SendMessage(_message, _param, SendMessageOptions.DontRequireReceiver);
        }
        _eventCount++;

        if (_isEventDestroy == true) 
        {
            Destroy(gameObject);
        }
    }
}
