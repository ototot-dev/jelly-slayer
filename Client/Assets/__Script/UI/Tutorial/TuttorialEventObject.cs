using UnityEngine;

public class TutorialEventObject : MonoBehaviour
{
    [SerializeField] TutorialManager _manager;
    [SerializeField] BoxCollider _boxCollider;

    float _scale = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localScale = _scale * Vector3.one;
    }
    public void OnTriggerEnter(Collider other)
    {
        CheckArrive(other.transform.position);
    }
    private void OnTriggerStay(Collider other)
    {
        CheckArrive(other.transform.position);
    }
    void CheckArrive(Vector3 pos) 
    {
        var dist = Vector3.Distance(transform.position, pos);
        if (dist > 1.0f)
            return;

        _boxCollider.enabled = false;

        _manager.GoNext();

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (_scale < 1.0f)
        {
            _scale += 3.0f * Time.deltaTime;
            if(_scale > 1.0f) 
            {
                _scale = 1.0f;
            }
            transform.localScale = _scale * Vector3.one;
        }
    }
}
