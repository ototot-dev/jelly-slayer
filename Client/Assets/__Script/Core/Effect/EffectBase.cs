using System.Collections;
using System.Collections.Generic;
using Unity.Linq;
using UnityEngine;

public class EffectBase : MonoBehaviour
{
    [SerializeField]
    bool _isAutoDestroy = true;

    [SerializeField]
    float _lifeTime = 3;

    // Start is called before the first frame update
    void Start()
    {
        if (_isAutoDestroy) 
        { 
            Invoke("Die", _lifeTime);
        }
    }
    void Die() 
    { 
        Destroy(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
