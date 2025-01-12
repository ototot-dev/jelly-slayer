using UnityEngine;

public class TutorialEventObject : MonoBehaviour
{
    [SerializeField] TutorialManager _manager;
    [SerializeField] BoxCollider _boxCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    public void OnTriggerEnter(Collider other)
    {
        _boxCollider.enabled = false;

        _manager.GoNext();

        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
