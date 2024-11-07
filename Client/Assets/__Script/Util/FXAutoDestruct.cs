using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class FXAutoDestruct : MonoBehaviour
{
    // If true, deactivate the object instead of destroying it
    public bool OnlyDeactivate;

    public bool UseLifeTime = false;
    public float lifeTime = 3;

    void OnEnable()
    {
        StartCoroutine("CheckIfAlive");
    }
    void Die()
    {
        if (OnlyDeactivate)
        {
#if UNITY_3_5
			this.gameObject.SetActiveRecursively(false);
#else
            this.gameObject.SetActive(false);
#endif
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
    IEnumerator CheckIfAlive()
    {
        ParticleSystem ps = this.GetComponent<ParticleSystem>();

        while (true && ps != null)
        {
            yield return new WaitForSeconds(0.5f);
            if (!ps.IsAlive(true))
            {
                Die();
                break;
            }
            if (UseLifeTime)
            {
                lifeTime -= 0.5f;
                if (lifeTime < 0)
                {
                    Die();
                    break;
                }
            }
        }
    }
}

