using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SubSlider : MonoBehaviour
{
    [SerializeField] private RectTransform m_SrcRT;
    [SerializeField] private RectTransform m_TargetRT;

    private Coroutine _followCoroutine = null;

    private float _time = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float dist = m_SrcRT.anchorMax.x - m_TargetRT.anchorMax.x;
        if (Mathf.Abs(dist) > Mathf.Epsilon)
        {
            _time += Time.deltaTime;
            if (_time >= 1.0f)
            {
                float max = 0.05f * dist;
                m_TargetRT.anchorMax += new Vector2(max, 0);
            }
        }
        else 
        {
            _time = 0;
        }
    }
}
